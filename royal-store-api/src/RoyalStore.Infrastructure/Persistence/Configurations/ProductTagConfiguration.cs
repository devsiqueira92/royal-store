using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.ToTable("ProductTags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ProductId).IsRequired();

        builder.Property(t => t.CategoryId).IsRequired();

        builder.Property(t => t.TenantId).IsRequired();

        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.ProductId, t.CategoryId })
            .IsUnique();
    }
}
