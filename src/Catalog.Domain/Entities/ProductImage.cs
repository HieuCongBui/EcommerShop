using Infrastructure.Shared.Common;

namespace Catalog.Domain.Entities;

/// <summary>
/// Represents a product image
/// </summary>
public class ProductImage : BaseEntity
{
    /// <summary>
    /// Image URL
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Alternative text for accessibility
    /// </summary>
    public string? AltText { get; set; }
    
    /// <summary>
    /// Sort order for display
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether this is the main image
    /// </summary>
    public bool IsMain { get; set; } = false;
    
    /// <summary>
    /// Product this image belongs to
    /// </summary>
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}