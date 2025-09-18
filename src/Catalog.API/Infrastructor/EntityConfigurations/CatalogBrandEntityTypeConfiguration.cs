namespace Catalog.API.Infrastructor.EntityConfigurations
{
    class CatalogBrandEntityTypeConfiguration
        : IEntityTypeConfiguration<CatalogBrand>
    {
        public void Configure(EntityTypeBuilder<CatalogBrand> builder)
        {
            builder.ToTable("CatalogBrands");

            builder.Property(cb => cb.Brand)
                .HasMaxLength(100);
        }
    }
}
