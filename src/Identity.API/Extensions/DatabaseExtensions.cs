using Identity.API.Data;
using Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace Identity.API.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var clientManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        // Ensure database is created and migrated
        await context.Database.MigrateAsync();

        // Seed data
        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedOpenIddictScopesAsync(scopeManager);
        await SeedOpenIddictClientsAsync(clientManager);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        var roles = new[]
        {
            new ApplicationRole 
            { 
                Name = "Admin", 
                Description = "Administrator role with full access",
                CreatedAt = DateTime.UtcNow
            },
            new ApplicationRole 
            { 
                Name = "User", 
                Description = "Standard user role",
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName: role.Name!))
            {
                await roleManager.CreateAsync(role);
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Create default admin user
        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    private static async Task SeedOpenIddictScopesAsync(IOpenIddictScopeManager scopeManager)
    {
        var scopes = new[]
        {
            new OpenIddictScopeDescriptor
            {
                Name = OpenIddictConstants.Scopes.OpenId,
                DisplayName = "OpenID Connect",
                Description = "OpenID Connect scope"
            },
            new OpenIddictScopeDescriptor
            {
                Name = OpenIddictConstants.Scopes.Profile,
                DisplayName = "Profile",
                Description = "Access to user profile information"
            },
            new OpenIddictScopeDescriptor
            {
                Name = OpenIddictConstants.Scopes.Email,
                DisplayName = "Email",
                Description = "Access to user email address"
            },
            new OpenIddictScopeDescriptor
            {
                Name = "roles",
                DisplayName = "Roles",
                Description = "Access to user roles and permissions"
            },
            new OpenIddictScopeDescriptor
            {
                Name = "catalog",
                DisplayName = "Catalog API",
                Description = "Access to catalog API",
                Resources = { "catalog-api" }
            }
        };

        foreach (var scope in scopes)
        {
            if (await scopeManager.FindByNameAsync(scope.Name!) == null)
            {
                await scopeManager.CreateAsync(scope);
            }
        }
    }

    private static async Task SeedOpenIddictClientsAsync(IOpenIddictApplicationManager clientManager)
    {
        // Web Application Client
        var webClientDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "web-client",
            ClientSecret = "web-client-secret",
            ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
            DisplayName = "Web Application",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            PostLogoutRedirectUris =
            {
                new Uri("https://localhost:7001/signout-callback-oidc")
            },
            RedirectUris =
            {
                new Uri("https://localhost:7001/signin-oidc")
            },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Prefixes.Scope + "roles",
                OpenIddictConstants.Permissions.Prefixes.Scope + "catalog"
            },
            Requirements =
            {
                OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
            }
        };

        if (await clientManager.FindByClientIdAsync("web-client") == null)
        {
            await clientManager.CreateAsync(webClientDescriptor);
        }

        // Mobile Application Client
        var mobileClientDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "mobile-client",
            ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
            DisplayName = "Mobile Application",
            ClientType = OpenIddictConstants.ClientTypes.Public,
            RedirectUris =
            {
                new Uri("com.ecommershop.mobile://callback")
            },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Prefixes.Scope + "roles",
                OpenIddictConstants.Permissions.Prefixes.Scope + "catalog"
            },
            Requirements =
            {
                OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
            }
        };

        if (await clientManager.FindByClientIdAsync("mobile-client") == null)
        {
            await clientManager.CreateAsync(mobileClientDescriptor);
        }

        // Swagger UI Client
        var swaggerClientDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "swagger-ui",
            ClientSecret = "swagger-ui-secret",
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            DisplayName = "Swagger UI",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            RedirectUris =
            {
                new Uri("https://localhost:7001/swagger/oauth2-redirect.html")
            },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Prefixes.Scope + "roles",
                OpenIddictConstants.Permissions.Prefixes.Scope + "catalog"
            },
            Requirements =
            {
                OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
            }
        };

        if (await clientManager.FindByClientIdAsync("swagger-ui") == null)
        {
            await clientManager.CreateAsync(swaggerClientDescriptor);
        }
    }
}