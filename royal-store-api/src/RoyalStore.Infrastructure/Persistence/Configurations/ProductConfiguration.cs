using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId).IsRequired();

        builder.Property(p => p.CategoryId).IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Sku)
            .HasMaxLength(50);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.CostPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.StockQuantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(p => p.MinStockQuantity)
            .HasPrecision(18, 3);

        builder.Property(p => p.IsStockTracked).IsRequired();

        builder.Property(p => p.IsActive).IsRequired();

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(2048);

        builder.Property(p => p.UnitOfMeasure)
            .HasMaxLength(6)
            .IsRequired()
            .HasDefaultValue("UN");

        builder.Property(p => p.Barcode)
            .HasMaxLength(14);

        builder.Property(p => p.Ncm)
            .HasMaxLength(8);

        builder.Property(p => p.Cest)
            .HasMaxLength(7);

        builder.Property(p => p.Origin).IsRequired();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
        builder.Property(p => p.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.TenantId, p.CategoryId });

        builder.HasIndex(p => new { p.TenantId, p.Sku })
            .IsUnique()
            .HasFilter("[Sku] IS NOT NULL");
    }
}
