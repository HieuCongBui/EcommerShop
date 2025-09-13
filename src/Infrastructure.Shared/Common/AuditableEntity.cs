namespace Infrastructure.Shared.Common;

/// <summary>
/// Base class for entities with audit information
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// User who created the entity
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// User who last updated the entity
    /// </summary>
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// User who deleted the entity
    /// </summary>
    public string? DeletedBy { get; set; }
}