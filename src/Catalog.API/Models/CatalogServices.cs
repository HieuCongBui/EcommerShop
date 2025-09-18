namespace Catalog.API.Models
{
    public class CatalogServices(
     CatalogDbContext context,
     IOptions<CatalogOptions> options,
     ILogger<CatalogServices> logger)
    {
        public CatalogDbContext Context { get; } = context;
        public IOptions<CatalogOptions> Options { get; } = options;
        public ILogger<CatalogServices> Logger { get; } = logger;
    }
}
