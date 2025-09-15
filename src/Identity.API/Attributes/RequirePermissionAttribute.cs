using Microsoft.AspNetCore.Authorization;

namespace Identity.API.Attributes;

public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = $"Permission.{permission}";
    }
}