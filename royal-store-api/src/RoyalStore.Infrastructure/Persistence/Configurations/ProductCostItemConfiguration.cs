using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ProductCostItemConfiguration : IEntityTypeConfiguration<ProductCostItem>
{
    public void Configure(EntityTypeBuilder<ProductCostItem> builder)
    {
        builder.ToTable("ProductCostItems", table =>
            table.HasCheckConstraint("CK_ProductCostItems_Value_NonNegative", "\"Value\" >= 0"));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ProductId).IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(ProductCostItem.DescriptionMaxLength)
            .IsRequired();

        builder.Property(e => e.Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.SortOrder).IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        // No unique index on (ProductId, SortOrder): a reorder renumbers lines inside one
        // SaveChanges and would hit transient duplicates (063 research D4).
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.TenantId);
    }
}
