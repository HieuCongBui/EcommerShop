namespace Catalog.API.Infrastructor
{
    public partial class CatalogDbSeeds(
        IWebHostEnvironment env,
        IOptions<CatalogOptions> settings,
        ILogger<CatalogDbSeeds> logger) : IDbSeeder<CatalogDbContext>
    {
        public Task SeedAsync(CatalogDbContext context)
        {
            throw new NotImplementedException();
        }
    }
}
