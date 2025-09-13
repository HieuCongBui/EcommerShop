using Infrastructure.Shared.Common;

namespace Identity.Domain.Entities;

/// <summary>
/// Represents a user in the identity system
/// </summary>
public class User : AuditableEntity
{
    /// <summary>
    /// User's email address (also serves as username)
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Normalized email for search and comparison
    /// </summary>
    public string NormalizedEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Whether the email is confirmed
    /// </summary>
    public bool EmailConfirmed { get; set; } = false;
    
    /// <summary>
    /// Whether the phone number is confirmed
    /// </summary>
    public bool PhoneNumberConfirmed { get; set; } = false;
    
    /// <summary>
    /// Password hash
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Security stamp for invalidating tokens
    /// </summary>
    public string SecurityStamp { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether two-factor authentication is enabled
    /// </summary>
    public bool TwoFactorEnabled { get; set; } = false;
    
    /// <summary>
    /// Number of failed login attempts
    /// </summary>
    public int AccessFailedCount { get; set; } = 0;
    
    /// <summary>
    /// Whether the account is locked out
    /// </summary>
    public bool LockoutEnabled { get; set; } = true;
    
    /// <summary>
    /// Lockout end date (null if not locked out)
    /// </summary>
    public DateTime? LockoutEnd { get; set; }
    
    /// <summary>
    /// Date and time of last login
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// User roles
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    
    /// <summary>
    /// User claims
    /// </summary>
    public ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();
    
    /// <summary>
    /// Gets the full name of the user
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}