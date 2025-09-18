namespace Catalog.API.Infrastructor.EntityConfigurations
{
    class CatalogTypeEntityTypeConfiguration
        :IEntityTypeConfiguration<CatalogType>
    {
        public void Configure(EntityTypeBuilder<CatalogType> builder)
        {
            builder.ToTable("CatalogTypes");
            builder.Property(ct => ct.Type)
                .HasMaxLength(100);
        }
    }
}
