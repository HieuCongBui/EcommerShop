using Infrastructure.Shared.Common;

namespace Catalog.Domain.Entities;

/// <summary>
/// Represents a product category in the catalog
/// </summary>
public class Category : AuditableEntity
{
    /// <summary>
    /// Category name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Category description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Category slug for URL
    /// </summary>
    public string Slug { get; set; } = string.Empty;
    
    /// <summary>
    /// Category image URL
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Parent category (for hierarchical categories)
    /// </summary>
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    
    /// <summary>
    /// Child categories
    /// </summary>
    public ICollection<Category> ChildCategories { get; set; } = new List<Category>();
    
    /// <summary>
    /// Products in this category
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
    
    /// <summary>
    /// Sort order for display
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether the category is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}