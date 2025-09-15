using Identity.API.Models;
using System.Security.Claims;

namespace Identity.API.Services;

public interface IClaimsService
{
    Task<IEnumerable<Claim>> GetUserClaimsAsync(ApplicationUser user);
    Task<ClaimsIdentity> CreateIdentityAsync(ApplicationUser user, IEnumerable<string> scopes);
}