namespace Ordering.API.Infrastructure.Services
{
    public class IdentityService(IHttpContextAccessor httpContext) : IIdentityService
    {
        public string GetUserIdentity() 
            => httpContext.HttpContext?.User.FindFirst("sub")?.Value;

        public string GetUserName()
            => httpContext.HttpContext?.User.Identity?.Name;
    }
}
