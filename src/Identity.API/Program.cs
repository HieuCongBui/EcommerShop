using Identity.API.Authorization;
using Identity.API.Data;
using Identity.API.Extensions;
using Identity.API.Models;
using Identity.API.Models.Configuration;
using Identity.API.Services;
using Identity.API.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Entity Framework with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityDb"));
    
    // Configure OpenIddict to use Entity Framework Core stores and models
    options.UseOpenIddict();
});

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Email confirmation settings
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddPasswordValidator<CustomPasswordValidator>();

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    var authorizationPolicyService = new AuthorizationPolicyService();
    authorizationPolicyService.ConfigurePolicies(options);
});

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationPolicyService, AuthorizationPolicyService>();

// Add Memory Cache for rate limiting
builder.Services.AddMemoryCache();

// Configure email settings and services
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailRateLimitingService, EmailRateLimitingService>();
builder.Services.AddScoped<Identity.API.Services.Templates.IEmailTemplateService, Identity.API.Services.Templates.EmailTemplateService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Configure Claims service
builder.Services.AddScoped<IClaimsService, ClaimsService>();

// Configure Password Security service
builder.Services.AddScoped<IPasswordSecurityService, PasswordSecurityService>();

// Configure OpenIddict server
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        // Enable the authorization, logout, token and userinfo endpoints
        options.SetAuthorizationEndpointUris("/connect/authorize")
               .SetEndSessionEndpointUris("/connect/logout")
               .SetTokenEndpointUris("/connect/token")
               .SetUserInfoEndpointUris("/connect/userinfo");

        // Mark the scopes as supported scopes
        options.RegisterScopes("openid", "profile", "email", "roles", "catalog");

        // Configure token lifetimes
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(jwtSettings.AccessTokenExpirationMinutes))
               .SetRefreshTokenLifetime(TimeSpan.FromDays(jwtSettings.RefreshTokenExpirationDays))
               .SetAuthorizationCodeLifetime(TimeSpan.FromMinutes(jwtSettings.AuthorizationCodeExpirationMinutes))
               .SetIdentityTokenLifetime(TimeSpan.FromMinutes(jwtSettings.IdentityTokenExpirationMinutes));

        // Register the signing and encryption credentials
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Configure supported flows
        options.AllowAuthorizationCodeFlow()
               .AllowClientCredentialsFlow()
               .AllowRefreshTokenFlow();

        // Require PKCE for public clients
        options.RequireProofKeyForCodeExchange();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserInfoEndpointPassthrough()
               .EnableStatusCodePagesIntegration();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// Add Razor Pages support for frontend with antiforgery protection
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Views/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Views/Account/Consent");
    options.Conventions.AllowAnonymousToPage("/Views/Account/Logout");
    options.Conventions.AllowAnonymousToPage("/Views/Account/Error");
});

// Configure Antiforgery with enhanced security
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddControllers();

// Configure Swagger with OAuth2 support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Identity API",
        Version = "v1",
        Description = "Identity and OAuth2 Authorization Server"
    });

    // Add OAuth2 security definition
    options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
        Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows
        {
            AuthorizationCode = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("/connect/authorize", UriKind.Relative),
                TokenUrl = new Uri("/connect/token", UriKind.Relative),
                Scopes = new Dictionary<string, string>
                {
                    ["openid"] = "OpenID Connect",
                    ["profile"] = "User profile",
                    ["email"] = "User email",
                    ["roles"] = "User roles",
                    ["catalog"] = "Catalog access"
                }
            }
        }
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "openid", "profile", "email", "roles", "catalog" }
        }
    });
});

var app = builder.Build();

// Initialize database
await app.Services.InitializeDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
        options.OAuthClientId("swagger-ui");
        options.OAuthClientSecret("swagger-ui-secret");
        options.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();

// Configure static files for frontend resources
app.UseStaticFiles();

// Configure security headers
app.Use(async (context, next) =>
{
    // Add security headers
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Add Content Security Policy
    var csp = "default-src 'self'; " +
              "script-src 'self' 'unsafe-inline'; " +
              "style-src 'self' 'unsafe-inline'; " +
              "img-src 'self' data:; " +
              "font-src 'self'; " +
              "connect-src 'self'; " +
              "frame-ancestors 'none';";
    context.Response.Headers.Add("Content-Security-Policy", csp);

    await next();
});

// Configure Identity cookie authentication
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    Secure = CookieSecurePolicy.SameAsRequest,
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always
});

app.UseAuthentication();
app.UseAuthorization();

// Configure routing for frontend pages
app.MapGet("/login", async context =>
{
    var returnUrl = context.Request.Query["returnUrl"].ToString();
    if (string.IsNullOrEmpty(returnUrl))
    {
        returnUrl = "/";
    }
    context.Response.Redirect($"/Views/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
});

app.MapGet("/consent", async context =>
{
    var returnUrl = context.Request.Query["returnUrl"].ToString();
    if (string.IsNullOrEmpty(returnUrl))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Missing returnUrl parameter");
        return;
    }
    context.Response.Redirect($"/Views/Account/Consent?returnUrl={Uri.EscapeDataString(returnUrl)}");
});

app.MapGet("/logout", async context =>
{
    var logoutId = context.Request.Query["logoutId"].ToString();
    var postLogoutRedirectUri = context.Request.Query["post_logout_redirect_uri"].ToString();
    
    var queryParams = new List<string>();
    if (!string.IsNullOrEmpty(logoutId))
        queryParams.Add($"logoutId={Uri.EscapeDataString(logoutId)}");
    if (!string.IsNullOrEmpty(postLogoutRedirectUri))
        queryParams.Add($"post_logout_redirect_uri={Uri.EscapeDataString(postLogoutRedirectUri)}");
    
    var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
    context.Response.Redirect($"/Views/Account/Logout{queryString}");
});

// Default OAuth2 endpoints redirect to login pages when accessed directly
// app.MapGet("/connect/authorize", async context =>
// {
//     // For GET requests to authorize endpoint, redirect to login page with all query parameters
//     var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
//     context.Response.Redirect($"/login{queryString}");
// });

app.MapRazorPages();
app.MapControllers();

app.Run();
