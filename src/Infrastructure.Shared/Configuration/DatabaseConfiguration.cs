using Microsoft.Extensions.Configuration;

namespace Infrastructure.Shared.Configuration;

/// <summary>
/// Configuration class for database settings
/// </summary>
public class DatabaseConfiguration
{
    public const string SectionName = "Database";
    
    /// <summary>
    /// Database provider name (PostgreSQL, SqlServer, MySQL, etc.)
    /// </summary>
    public string Provider { get; set; } = "PostgreSQL";
    
    /// <summary>
    /// Dictionary of connection strings by database name
    /// </summary>
    public Dictionary<string, string> ConnectionStrings { get; set; } = new();
    
    /// <summary>
    /// Default connection string key
    /// </summary>
    public string DefaultConnectionKey { get; set; } = "DefaultConnection";
    
    /// <summary>
    /// Whether to run migrations automatically on startup
    /// </summary>
    public bool AutoMigrate { get; set; } = true;
    
    /// <summary>
    /// Whether to seed data automatically after migrations
    /// </summary>
    public bool AutoSeed { get; set; } = true;
    
    /// <summary>
    /// Migration timeout in seconds
    /// </summary>
    public int MigrationTimeoutSeconds { get; set; } = 300;
    
    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Retry count for failed operations
    /// </summary>
    public int RetryCount { get; set; } = 3;
    
    /// <summary>
    /// Environment-specific settings
    /// </summary>
    public Dictionary<string, object> EnvironmentSettings { get; set; } = new();
}