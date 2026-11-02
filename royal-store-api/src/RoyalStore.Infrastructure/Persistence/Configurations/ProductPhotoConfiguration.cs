using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ProductPhotoConfiguration : IEntityTypeConfiguration<ProductPhoto>
{
    public void Configure(EntityTypeBuilder<ProductPhoto> builder)
    {
        builder.ToTable("ProductPhotos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProductId).IsRequired();
        builder.Property(p => p.TenantId).IsRequired();

        builder.Property(p => p.Url)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(p => p.IsCover).IsRequired();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
        builder.Property(p => p.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasIndex(p => new { p.ProductId, p.IsCover })
            .IsUnique()
            .HasFilter("\"IsCover\" = true");

        builder.HasIndex(p => p.TenantId);
    }
}
