using Infrastructure.Shared.Common;

namespace Identity.Domain.Entities;

/// <summary>
/// Represents the many-to-many relationship between users and roles
/// </summary>
public class UserRole : BaseEntity
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Role ID
    /// </summary>
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    
    /// <summary>
    /// Date when the role was assigned
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User who assigned the role
    /// </summary>
    public string? AssignedBy { get; set; }
}