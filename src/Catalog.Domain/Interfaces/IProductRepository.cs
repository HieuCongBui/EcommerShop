using Catalog.Domain.Entities;

namespace Catalog.Domain.Interfaces;

/// <summary>
/// Repository interface for Product entity
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets all products
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of products</returns>
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a product by its ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product or null if not found</returns>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a product by its SKU
    /// </summary>
    /// <param name="sku">Product SKU</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product or null if not found</returns>
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets products by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of products in the category</returns>
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new product
    /// </summary>
    /// <param name="product">Product to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Added product</returns>
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="product">Product to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product</returns>
    Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}