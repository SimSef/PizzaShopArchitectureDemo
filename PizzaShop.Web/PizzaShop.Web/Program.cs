using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Yarp.ReverseProxy.Transforms;
using PizzaShop.Orleans.Contract;
using PizzaShop.Web;
using PizzaShop.Web.Components;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;

const string CorsPolicyName = "bff";

var builder = WebApplication.CreateBuilder(args);

// Explicitly register keyed Azure clients using Aspire-provided connection strings.
// Orleans expects keyed services matching the configured ServiceKey names.
string GetConnection(string name) =>
    builder.Configuration.GetConnectionString(name)
    ?? throw new InvalidOperationException($"Missing connection string '{name}'.");

builder.Services.AddKeyedSingleton<TableServiceClient>("orleans-clustering",
    (_, _) => new TableServiceClient(GetConnection("orleans-clustering")));
builder.Services.AddKeyedSingleton<TableServiceClient>("orleans-reminders",
    (_, _) => new TableServiceClient(GetConnection("orleans-reminders")));
builder.Services.AddKeyedSingleton<BlobServiceClient>("orleans-grainstate",
    (_, _) => new BlobServiceClient(GetConnection("orleans-grainstate")));
builder.Services.AddKeyedSingleton<QueueServiceClient>("orleans-streams",
    (_, _) => new QueueServiceClient(GetConnection("orleans-streams")));
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
        cookieOptions.Cookie.SameSite = SameSiteMode.Strict;
        cookieOptions.Cookie.Path = "/";
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = keycloakAuthority;

        if (builder.Environment.IsDevelopment()
            && keycloakAuthority.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            options.RequireHttpsMetadata = false;
        }

        options.ClientId = keycloakClientId;
        options.ClientSecret = keycloakClientSecret;

        options.ResponseType = OpenIdConnectResponseType.Code;
        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";

        // BFF keeps tokens server-side in the auth ticket.
        options.SaveTokens = true;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });

// ConfigureCookieOidc attaches a cookie OnValidatePrincipal callback to get
// a new access token when the current one expires, and reissue a cookie with the
// new access token saved inside. OIDC options here already save tokens; the
// extension adds offline_access and refresh behavior.
builder.Services.ConfigureCookieOidc(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);

builder.Services.AddAuthorizationBuilder();

builder.Services.AddCascadingAuthenticationState();

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
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PizzaShop.Web.Client._Imports).Assembly);

app.MapPost("/authentication/logout", (HttpContext httpContext, ILogger<Program> logger) =>
    {
        logger.LogInformation(
            "Logout endpoint hit. IsAuthenticated={IsAuthenticated}, User={User}",
            httpContext.User?.Identity?.IsAuthenticated,
            httpContext.User?.Identity?.Name ?? "<anonymous>");

        var props = new AuthenticationProperties
        {
            RedirectUri = spaOrigin
        };

        return Results.SignOut(
            props,
            [
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            ]);
    });

// Simple Orleans counter endpoint for testing the cluster.
app.MapGet("/api/counter/increment", async (IGrainFactory grains) =>
{
    var grain = grains.GetGrain<ICounterGrain>("counter");
    var count = await grain.IncrementAsync();
    return Results.Ok(new { count });
});

app.Run();
