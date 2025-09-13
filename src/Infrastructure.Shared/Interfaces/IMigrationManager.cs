namespace Infrastructure.Shared.Interfaces;

/// <summary>
/// Interface for managing database migrations
/// </summary>
public interface IMigrationManager
{
    /// <summary>
    /// Applies pending migrations to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the migration operation</returns>
    Task ApplyMigrationsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if there are pending migrations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if there are pending migrations, false otherwise</returns>
    Task<bool> HasPendingMigrationsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the list of applied migrations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of applied migration names</returns>
    Task<IEnumerable<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the list of pending migrations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pending migration names</returns>
    Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates database connection
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection is valid, false otherwise</returns>
    Task<bool> ValidateConnectionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Seeds initial data if needed
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the seed operation</returns>
    Task SeedDataAsync(CancellationToken cancellationToken = default);
}