namespace Infrastructure.Shared.Interfaces;

/// <summary>
/// Interface for providing database connection strings
/// </summary>
public interface IConnectionStringProvider
{
    /// <summary>
    /// Gets the connection string for the specified database name
    /// </summary>
    /// <param name="databaseName">The name of the database</param>
    /// <returns>The connection string</returns>
    string GetConnectionString(string databaseName);
    
    /// <summary>
    /// Gets the default connection string
    /// </summary>
    /// <returns>The default connection string</returns>
    string GetDefaultConnectionString();
    
    /// <summary>
    /// Validates if a connection string is properly configured
    /// </summary>
    /// <param name="databaseName">The name of the database</param>
    /// <returns>True if connection string is valid, false otherwise</returns>
    bool IsConnectionStringConfigured(string databaseName);
}