using Infrastructure.Shared.Common;

namespace Identity.Domain.Entities;

/// <summary>
/// Represents a role in the identity system
/// </summary>
public class Role : AuditableEntity
{
    /// <summary>
    /// Role name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Normalized role name for search and comparison
    /// </summary>
    public string NormalizedName { get; set; } = string.Empty;
    
    /// <summary>
    /// Role description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether the role is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether this is a system role (cannot be deleted)
    /// </summary>
    public bool IsSystemRole { get; set; } = false;
    
    /// <summary>
    /// Users assigned to this role
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    
    /// <summary>
    /// Claims associated with this role
    /// </summary>
    public ICollection<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();
}