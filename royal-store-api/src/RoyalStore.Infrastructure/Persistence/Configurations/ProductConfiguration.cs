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

        builder.Property(p => p.IsActive).IsRequired();

        builder.Property(p => p.UnitOfMeasure)
            .HasMaxLength(6)
            .IsRequired()
            .HasDefaultValue("UN");

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

        builder.HasMany(p => p.Variants)
            .WithOne()
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Tags)
            .WithOne()
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Photos)
            .WithOne()
            .HasForeignKey(ph => ph.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.TenantId, p.CategoryId });
    }
}
