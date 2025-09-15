using Microsoft.AspNetCore.Authorization;

namespace Identity.API.Attributes;

public class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}