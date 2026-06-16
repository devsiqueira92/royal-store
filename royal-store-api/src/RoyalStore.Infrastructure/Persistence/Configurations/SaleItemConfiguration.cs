using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SaleId).IsRequired();
        builder.Property(e => e.ProductId).IsRequired();

        builder.Property(e => e.Quantity)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(e => e.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.TotalPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.Ncm)
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(e => e.Cest)
            .HasMaxLength(7);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.SaleId);
    }
}
