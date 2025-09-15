using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Identity.API.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var user = context.User;
        
        if (user?.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        // Check if user has the required permission
        if (user.HasClaim("permission", requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user is admin (admin has all permissions)
        if (user.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}