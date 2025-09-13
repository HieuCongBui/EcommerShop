using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Shared.Interfaces;

/// <summary>
/// Factory interface for creating database contexts
/// </summary>
/// <typeparam name="TContext">The type of DbContext</typeparam>
public interface IDbContextFactory<TContext> where TContext : DbContext
{
    /// <summary>
    /// Creates a new instance of the database context
    /// </summary>
    /// <returns>A new database context instance</returns>
    TContext CreateDbContext();
    
    /// <summary>
    /// Creates a new instance of the database context with a specific connection string
    /// </summary>
    /// <param name="connectionString">The connection string to use</param>
    /// <returns>A new database context instance</returns>
    TContext CreateDbContext(string connectionString);
}