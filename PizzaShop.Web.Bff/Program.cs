using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using PizzaShop.Orleans.Contract;
using PizzaShop.Web.Bff;

const string CorsPolicyName = "bff";

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability();

var authActivitySource = new ActivitySource("PizzaShop.Web.Auth");

// Register keyed Azure clients using Aspire helpers; Orleans expects the keys to
// match the configured ServiceKey names.
builder.AddKeyedAzureTableServiceClient("orleans-clustering");
builder.AddKeyedAzureTableServiceClient("orleans-reminders");
builder.AddKeyedAzureBlobServiceClient("orleans-grainstate");
builder.AddKeyedAzureQueueServiceClient("orleans-streams");
builder.UseOrleansClient();

// SPA origin for BFF CORS (required, no defaults).
var spaOrigin = builder.Configuration["Spa:Origin"];
if (string.IsNullOrWhiteSpace(spaOrigin))
{
    throw new InvalidOperationException("Missing required configuration for SPA origin. Set 'Spa__Origin' (via Aspire AppHost or environment).");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(spaOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Keycloak configuration via environment variables (BFF pattern) — all required, no defaults.
var keycloakAuthority = builder.Configuration["Keycloak:Authority"];
if (string.IsNullOrWhiteSpace(keycloakAuthority))
{
    throw new InvalidOperationException("Missing required configuration 'Keycloak__Authority'.");
}

var keycloakClientId = builder.Configuration["Keycloak:ClientId"];
if (string.IsNullOrWhiteSpace(keycloakClientId))
{
    throw new InvalidOperationException("Missing required configuration 'Keycloak__ClientId'.");
}

var keycloakClientSecret = builder.Configuration["Keycloak:ClientSecret"];
if (string.IsNullOrWhiteSpace(keycloakClientSecret))
{
    throw new InvalidOperationException("Missing required configuration 'Keycloak__ClientSecret'.");
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(cookieOptions =>
    {
        cookieOptions.Cookie.Name = "__Host-pizzashop-auth";
        cookieOptions.Cookie.HttpOnly = true;
        cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        cookieOptions.Cookie.SameSite = SameSiteMode.None;
        cookieOptions.Cookie.Path = "/";
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = keycloakAuthority;

        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false; // allow HTTP for metadata in dev
        }

        options.ClientId = keycloakClientId;
        options.ClientSecret = keycloakClientSecret;

        options.ResponseType = OpenIdConnectResponseType.Code;
        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });

// Enable refresh tokens via offline_access so the CookieOidcRefresher can
// keep the authentication cookie valid without forcing the user to re-login.
builder.Services.ConfigureCookieOidc(
    CookieAuthenticationDefaults.AuthenticationScheme,
    OpenIdConnectDefaults.AuthenticationScheme);

builder.Services.AddAuthorizationBuilder();
builder.Services.AddAntiforgery();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Authentication endpoints used by the SPA for login/logout.
app.MapGet("/authentication/login", (HttpContext httpContext) =>
{
    using var activity = authActivitySource.StartActivity("Auth.LoginEndpoint.Challenge");

    var returnUrl = httpContext.Request.Query["returnUrl"].ToString();
    if (string.IsNullOrWhiteSpace(returnUrl))
    {
        returnUrl = "/";
    }

    if (!returnUrl.StartsWith("/"))
    {
        returnUrl = "/";
    }

    var redirectUri = $"{spaOrigin.TrimEnd('/')}{returnUrl}";

    var props = new AuthenticationProperties
    {
        RedirectUri = redirectUri
    };

    activity?.SetTag("auth.return_url", returnUrl);
    activity?.SetTag("auth.user_authenticated", httpContext.User?.Identity?.IsAuthenticated ?? false);

    return Results.Challenge(
        props,
        [OpenIdConnectDefaults.AuthenticationScheme]);
}).AllowAnonymous();

app.MapMethods("/authentication/logout", ["GET", "POST"], (HttpContext httpContext) =>
{
    using var activity = authActivitySource.StartActivity("Auth.LogoutEndpoint.SignOut");

    var returnUrl = httpContext.Request.Query["returnUrl"].ToString();
    if (string.IsNullOrWhiteSpace(returnUrl))
    {
        returnUrl = "/";
    }

    if (!returnUrl.StartsWith("/"))
    {
        returnUrl = "/";
    }

    var redirectUri = $"{spaOrigin.TrimEnd('/')}{returnUrl}";

    var props = new AuthenticationProperties
    {
        RedirectUri = redirectUri
    };

    activity?.SetTag("auth.return_url", returnUrl);
    activity?.SetTag("auth.user_name", httpContext.User?.Identity?.Name ?? "<anonymous>");

    return Results.SignOut(
        props,
        [
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
        ]);
}).RequireAuthorization();

// Authenticated user info for BFF clients (no tokens, just claims).
app.MapGet("/api/me", async (HttpContext httpContext) =>
{
    var authenticateResult = await httpContext.AuthenticateAsync();
    if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
    {
        return Results.Unauthorized();
    }

    var claims = authenticateResult.Principal.Claims.Select(c => new { type = c.Type, value = c.Value });
    return Results.Ok(new { claims });
});

app.Run();
