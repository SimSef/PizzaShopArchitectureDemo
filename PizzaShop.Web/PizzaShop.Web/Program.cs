using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.Hosting;
using PizzaShop.Orleans.Contract;
using PizzaShop.Web;
using PizzaShop.Web.Components;
using Azure.Data.Tables;

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
var spaOrigin = builder.Configuration["Spa:Origin"] ?? builder.Configuration["Spa__Origin"];
if (string.IsNullOrWhiteSpace(spaOrigin))
{
    throw new InvalidOperationException("Missing required configuration for SPA origin. Set 'Spa__Origin' (or 'Spa:Origin') via Aspire AppHost or environment.");
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

// Keycloak configuration via env/config (BFF pattern) — all required, no defaults.
var keycloakAuthority = builder.Configuration["Keycloak:Authority"] ?? builder.Configuration["Keycloak__Authority"];
if (string.IsNullOrWhiteSpace(keycloakAuthority))
{
    throw new InvalidOperationException("Missing required configuration 'Keycloak__Authority' (or 'Keycloak:Authority').");
}

var keycloakClientId = builder.Configuration["Keycloak:ClientId"] ?? builder.Configuration["Keycloak__ClientId"];
if (string.IsNullOrWhiteSpace(keycloakClientId))
{
    throw new InvalidOperationException("Missing required configuration 'Keycloak__ClientId' (or 'Keycloak:ClientId').");
}

var keycloakClientSecret = builder.Configuration["Keycloak:ClientSecret"] ?? builder.Configuration["Keycloak__ClientSecret"];
if (string.IsNullOrWhiteSpace(keycloakClientSecret))
{
    throw new InvalidOperationException("Missing required configuration 'Keycloak__ClientSecret' (or 'Keycloak:ClientSecret').");
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
        cookieOptions.Cookie.SameSite = SameSiteMode.Lax;
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

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// Remove or set 'SerializeAllClaims' to 'false' if you only want to 
// serialize name and role claims for CSR.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// For server-side requests that hit a 404, redirect back to the home page.
app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
    {
        context.HttpContext.Response.Redirect("/");
    }
});
app.UseHttpsRedirection();

app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PizzaShop.Web.Client._Imports).Assembly);

// Authentication endpoints used by StartOrder/Logout pages.
app.MapGet("/authentication/login", (HttpContext httpContext) =>
{
    using var activity = authActivitySource.StartActivity("Auth.LoginEndpoint.Challenge");

    var returnUrl = httpContext.Request.Query["returnUrl"].ToString();
    if (string.IsNullOrWhiteSpace(returnUrl))
    {
        returnUrl = "/order";
    }

    var props = new AuthenticationProperties
    {
        RedirectUri = returnUrl
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

    var props = new AuthenticationProperties
    {
        RedirectUri = returnUrl
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

// Simple Orleans counter endpoint for testing the cluster.
app.MapGet("/api/counter/increment", async (IGrainFactory grains) =>
{
    var grain = grains.GetGrain<ICounterGrain>("counter");
    var count = await grain.IncrementAsync();
    return Results.Ok(new { count });
});

app.Run();
