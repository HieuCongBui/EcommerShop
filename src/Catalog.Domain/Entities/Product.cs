using Infrastructure.Shared.Common;

namespace Catalog.Domain.Entities;

/// <summary>
/// Represents a product in the catalog
/// </summary>
public class Product : AuditableEntity
{
    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Product description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Product SKU (Stock Keeping Unit)
    /// </summary>
    public string Sku { get; set; } = string.Empty;
    
    /// <summary>
    /// Product slug for URL
    /// </summary>
    public string Slug { get; set; } = string.Empty;
    
    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Product cost
    /// </summary>
    public decimal? Cost { get; set; }
    
    /// <summary>
    /// Currency code (USD, EUR, etc.)
    /// </summary>
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Stock quantity
    /// </summary>
    public int StockQuantity { get; set; } = 0;
    
    /// <summary>
    /// Minimum stock level for alerts
    /// </summary>
    public int? MinStockLevel { get; set; }
    
    /// <summary>
    /// Product weight in grams
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// Product dimensions (Length x Width x Height in cm)
    /// </summary>
    public string? Dimensions { get; set; }
    
    /// <summary>
    /// Main product image URL
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Additional product images
    /// </summary>
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    
    /// <summary>
    /// Product category
    /// </summary>
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    /// <summary>
    /// Whether the product is featured
    /// </summary>
    public bool IsFeatured { get; set; } = false;
    
    /// <summary>
    /// Whether the product is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Product tags for search and filtering
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// SEO meta title
    /// </summary>
    public string? MetaTitle { get; set; }
    
    /// <summary>
    /// SEO meta description
    /// </summary>
    public string? MetaDescription { get; set; }
}