using Catalog.Domain.Entities;

namespace Catalog.Domain.Interfaces;

/// <summary>
/// Repository interface for Category entity
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Gets all categories
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of categories</returns>
    Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a category by its ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category or null if not found</returns>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a category by its slug
    /// </summary>
    /// <param name="slug">Category slug</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category or null if not found</returns>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets root categories (categories without parent)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of root categories</returns>
    Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets child categories of a parent category
    /// </summary>
    /// <param name="parentId">Parent category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of child categories</returns>
    Task<IEnumerable<Category>> GetChildCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new category
    /// </summary>
    /// <param name="category">Category to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Added category</returns>
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing category
    /// </summary>
    /// <param name="category">Category to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated category</returns>
    Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}