using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ProductSizeConfiguration : IEntityTypeConfiguration<ProductSize>
{
    public void Configure(EntityTypeBuilder<ProductSize> builder)
    {
        builder.ToTable("ProductSizes");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ProductId).IsRequired();
        builder.Property(s => s.TenantId).IsRequired();

        builder.Property(s => s.Size)
            .HasConversion<string>()
            .HasMaxLength(5)
            .IsRequired();

        builder.HasIndex(s => new { s.ProductId, s.Size }).IsUnique();
        builder.HasIndex(s => s.TenantId);
    }
}
