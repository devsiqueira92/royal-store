using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.ProductId).IsRequired();

        builder.Property(v => v.TenantId).IsRequired();

        builder.Property(v => v.Size).HasMaxLength(50);

        builder.Property(v => v.Color).HasMaxLength(50);

        builder.Property(v => v.Sku).HasMaxLength(50);

        builder.Property(v => v.Barcode).HasMaxLength(14);

        builder.Property(v => v.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(v => v.CostPrice)
            .HasPrecision(18, 2);

        builder.Property(v => v.StockQuantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(v => v.MinStockQuantity)
            .HasPrecision(18, 3);

        builder.Property(v => v.IsStockTracked).IsRequired();

        builder.Property(v => v.PromotionalPrice).HasPrecision(18, 2);
        builder.Property(v => v.NetWeight).HasPrecision(9, 3);
        builder.Property(v => v.GrossWeight).HasPrecision(9, 3);
        builder.Property(v => v.Width).HasPrecision(9, 2);
        builder.Property(v => v.Height).HasPrecision(9, 2);
        builder.Property(v => v.Length).HasPrecision(9, 2);

        builder.HasIndex(v => new { v.TenantId, v.Sku })
            .IsUnique()
            .HasFilter("[Sku] IS NOT NULL");

        builder.HasIndex(v => v.ProductId);
    }
}
