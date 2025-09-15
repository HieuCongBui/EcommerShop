using Microsoft.AspNetCore.Authorization;

namespace Identity.API.Services;

public interface IAuthorizationPolicyService
{
    AuthorizationPolicy GetPermissionPolicy(string permission);
    AuthorizationPolicy GetRolePolicy(params string[] roles);
    void ConfigurePolicies(AuthorizationOptions options);
}