using Infrastructure.Shared.Common;

namespace Identity.Domain.Entities;

/// <summary>
/// Represents a claim associated with a user
/// </summary>
public class UserClaim : BaseEntity
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Claim type
    /// </summary>
    public string ClaimType { get; set; } = string.Empty;
    
    /// <summary>
    /// Claim value
    /// </summary>
    public string ClaimValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Claim issuer
    /// </summary>
    public string? Issuer { get; set; }
}