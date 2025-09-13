namespace Infrastructure.Shared.Interfaces;

/// <summary>
/// Interface for database provider abstraction
/// </summary>
public interface IDatabaseProvider
{
    /// <summary>
    /// Gets the provider name (e.g., "PostgreSQL", "SqlServer", "MySQL")
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// Configures the database context options for this provider
    /// </summary>
    /// <param name="optionsBuilder">The options builder</param>
    /// <param name="connectionString">The connection string</param>
    void ConfigureDbContext(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder, string connectionString);
    
    /// <summary>
    /// Gets provider-specific migration assembly name
    /// </summary>
    /// <returns>The migration assembly name</returns>
    string GetMigrationAssemblyName();
    
    /// <summary>
    /// Performs provider-specific database initialization
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the initialization</returns>
    Task InitializeDatabaseAsync(string connectionString, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tests database connection for this provider
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection is successful, false otherwise</returns>
    Task<bool> TestConnectionAsync(string connectionString, CancellationToken cancellationToken = default);
}