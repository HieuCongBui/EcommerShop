using Identity.API.Authorization;
using Identity.API.Models.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Identity.API.Services;

public class AuthorizationPolicyService : IAuthorizationPolicyService
{
    public AuthorizationPolicy GetPermissionPolicy(string permission)
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permission))
            .Build();
    }

    public AuthorizationPolicy GetRolePolicy(params string[] roles)
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new RoleRequirement(roles))
            .Build();
    }

    public void ConfigurePolicies(AuthorizationOptions options)
    {
        // Configure permission-based policies
        foreach (var permission in Permission.GetAllPermissions())
        {
            options.AddPolicy($"Permission.{permission}", GetPermissionPolicy(permission));
        }

        // Configure role-based policies
        options.AddPolicy("AdminOnly", GetRolePolicy(Roles.Admin));
        options.AddPolicy("UserOrAdmin", GetRolePolicy(Roles.User, Roles.Admin));

        // Configure specific business policies
        options.AddPolicy("CanManageUsers", policy =>
            policy.RequireAuthenticatedUser()
                  .AddRequirements(new PermissionRequirement(Permission.Users.Write)));

        options.AddPolicy("CanManageCatalog", policy =>
            policy.RequireAuthenticatedUser()
                  .AddRequirements(new PermissionRequirement(Permission.Catalog.Write)));

        options.AddPolicy("CanViewOrders", policy =>
            policy.RequireAuthenticatedUser()
                  .AddRequirements(new PermissionRequirement(Permission.Orders.Read)));

        options.AddPolicy("CanManageOrders", policy =>
            policy.RequireAuthenticatedUser()
                  .AddRequirements(new PermissionRequirement(Permission.Orders.Write)));

        options.AddPolicy("SystemAdmin", policy =>
            policy.RequireAuthenticatedUser()
                  .AddRequirements(new PermissionRequirement(Permission.System.Admin)));
    }
}