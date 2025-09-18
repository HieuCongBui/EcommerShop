namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static string GetRequiredValue(this IConfiguration configuration, string name)
            => configuration[name]?? throw new InvalidOperationException($"Configuration value '{name}' is required.");
    }
}
