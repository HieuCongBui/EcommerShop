using Microsoft.AspNetCore.Authorization;

namespace Identity.API.Authorization;

public class RoleRequirement : IAuthorizationRequirement
{
    public string[] Roles { get; }

    public RoleRequirement(params string[] roles)
    {
        Roles = roles ?? throw new ArgumentNullException(nameof(roles));
    }
}