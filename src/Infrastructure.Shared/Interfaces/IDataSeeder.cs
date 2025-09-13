namespace Infrastructure.Shared.Interfaces;

/// <summary>
/// Interface for seeding initial data
/// </summary>
public interface IDataSeeder
{
    /// <summary>
    /// Seeds initial data for the specified environment
    /// </summary>
    /// <param name="environment">The environment (Development, Staging, Production)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the seed operation</returns>
    Task SeedAsync(string environment, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the order in which this seeder should run
    /// </summary>
    int Order { get; }
    
    /// <summary>
    /// Determines if this seeder should run for the specified environment
    /// </summary>
    /// <param name="environment">The environment</param>
    /// <returns>True if seeder should run, false otherwise</returns>
    bool ShouldSeed(string environment);
}