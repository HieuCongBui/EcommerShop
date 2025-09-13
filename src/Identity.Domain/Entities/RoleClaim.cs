using Infrastructure.Shared.Common;

namespace Identity.Domain.Entities;

/// <summary>
/// Represents a claim associated with a role
/// </summary>
public class RoleClaim : BaseEntity
{
    /// <summary>
    /// Role ID
    /// </summary>
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    
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