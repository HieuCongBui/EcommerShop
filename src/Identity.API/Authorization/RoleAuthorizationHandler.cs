using Microsoft.AspNetCore.Authorization;

namespace Identity.API.Authorization;

public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        var user = context.User;
        
        if (user?.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        // Check if user has any of the required roles
        foreach (var role in requirement.Roles)
        {
            if (user.IsInRole(role))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}