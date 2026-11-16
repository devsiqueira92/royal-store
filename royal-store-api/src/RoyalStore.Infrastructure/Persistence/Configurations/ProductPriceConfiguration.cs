using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("ProductPrices");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ProductId).IsRequired();
        builder.Property(e => e.PriceListId).IsRequired();

        builder.Property(e => e.SalePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.MinPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        // Restrict, not Cascade: a list that still prices products can only be deactivated
        // (061 FR-006) — deleting it must never silently take those prices with it.
        builder.HasOne(e => e.PriceList)
            .WithMany()
            .HasForeignKey(e => e.PriceListId)
            .OnDelete(DeleteBehavior.Restrict);

        // One price per product per list (061 FR-008).
        builder.HasIndex(e => new { e.ProductId, e.PriceListId }).IsUnique();

        builder.HasIndex(e => new { e.TenantId, e.PriceListId });
    }
}
