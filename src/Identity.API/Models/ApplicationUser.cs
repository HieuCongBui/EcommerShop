using Microsoft.AspNetCore.Identity;

namespace Identity.API.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsEmailConfirmed => EmailConfirmed;
    public bool IsTwoFactorEnabled => TwoFactorEnabled;
}