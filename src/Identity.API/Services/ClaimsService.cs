using Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identity.API.Services;

public class ClaimsService : IClaimsService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ClaimsService> _logger;

    public ClaimsService(UserManager<ApplicationUser> userManager, ILogger<ClaimsService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IEnumerable<Claim>> GetUserClaimsAsync(ApplicationUser user)
    {
        var claims = new List<Claim>();

        try
        {
            // Standard OpenID Connect claims
            claims.Add(new Claim(Claims.Subject, user.Id));
            claims.Add(new Claim(Claims.Name, $"{user.FirstName} {user.LastName}".Trim()));
            claims.Add(new Claim(Claims.GivenName, user.FirstName ?? string.Empty));
            claims.Add(new Claim(Claims.FamilyName, user.LastName ?? string.Empty));
            claims.Add(new Claim(Claims.PreferredUsername, user.UserName ?? string.Empty));
            claims.Add(new Claim(Claims.Email, user.Email ?? string.Empty));
            claims.Add(new Claim(Claims.EmailVerified, user.EmailConfirmed.ToString().ToLower()));

            // Custom claims
            claims.Add(new Claim("user_id", user.Id));
            claims.Add(new Claim("created_at", user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            
            if (user.LastLoginAt.HasValue)
            {
                claims.Add(new Claim("last_login_at", user.LastLoginAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            }

            // Add roles
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(Claims.Role, role));
                claims.Add(new Claim("role", role)); // Alternative role claim
            }

            // Add permissions based on roles
            var permissions = GetPermissionsForRoles(roles);
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            // Add additional user claims from Identity system
            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            _logger.LogDebug("Generated {ClaimCount} claims for user {UserId}", claims.Count, user.Id);
            return claims;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating claims for user {UserId}", user.Id);
            throw;
        }
    }

    public async Task<ClaimsIdentity> CreateIdentityAsync(ApplicationUser user, IEnumerable<string> scopes)
    {
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        var userClaims = await GetUserClaimsAsync(user);
        
        // Filter claims based on requested scopes
        var filteredClaims = FilterClaimsByScopes(userClaims, scopes);
        identity.AddClaims(filteredClaims);

        // Set OpenIddict-specific properties
        identity.SetScopes(scopes);
        
        return identity;
    }

    private static IEnumerable<Claim> FilterClaimsByScopes(IEnumerable<Claim> claims, IEnumerable<string> scopes)
    {
        var scopeList = scopes.ToList();
        var filteredClaims = new List<Claim>();

        foreach (var claim in claims)
        {
            switch (claim.Type)
            {
                // Always include subject claim
                case Claims.Subject:
                case "user_id":
                    filteredClaims.Add(claim);
                    break;

                // Profile scope claims
                case Claims.Name:
                case Claims.GivenName:
                case Claims.FamilyName:
                case Claims.PreferredUsername:
                case "created_at":
                case "last_login_at":
                    if (scopeList.Contains(Scopes.Profile))
                        filteredClaims.Add(claim);
                    break;

                // Email scope claims
                case Claims.Email:
                case Claims.EmailVerified:
                    if (scopeList.Contains(Scopes.Email))
                        filteredClaims.Add(claim);
                    break;

                // Role scope claims
                case Claims.Role:
                case "permission":
                    if (scopeList.Contains("roles"))
                        filteredClaims.Add(claim);
                    break;

                // Catalog scope claims
                case "catalog_access":
                    if (scopeList.Contains("catalog"))
                        filteredClaims.Add(claim);
                    break;

                // Custom claims - include if any custom scope is requested
                default:
                    if (scopeList.Any(s => !StandardScopes.Contains(s)))
                        filteredClaims.Add(claim);
                    break;
            }
        }

        return filteredClaims;
    }

    private static IEnumerable<string> GetPermissionsForRoles(IEnumerable<string> roles)
    {
        var permissions = new HashSet<string>();

        foreach (var role in roles)
        {
            switch (role.ToLowerInvariant())
            {
                case "admin":
                    permissions.Add("users.read");
                    permissions.Add("users.write");
                    permissions.Add("users.delete");
                    permissions.Add("catalog.read");
                    permissions.Add("catalog.write");
                    permissions.Add("catalog.delete");
                    permissions.Add("orders.read");
                    permissions.Add("orders.write");
                    permissions.Add("orders.delete");
                    permissions.Add("system.admin");
                    break;

                case "user":
                    permissions.Add("catalog.read");
                    permissions.Add("orders.read");
                    permissions.Add("orders.write");
                    permissions.Add("profile.read");
                    permissions.Add("profile.write");
                    break;
            }
        }

        return permissions;
    }

    private static readonly HashSet<string> StandardScopes = new()
    {
        Scopes.OpenId,
        Scopes.Profile,
        Scopes.Email,
        "roles"
    };
}