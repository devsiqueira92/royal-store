using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class SalePaymentConfiguration : IEntityTypeConfiguration<SalePayment>
{
    public void Configure(EntityTypeBuilder<SalePayment> builder)
    {
        builder.ToTable("SalePayments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.SaleId).IsRequired();

        builder.Property(e => e.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Acquirer detail. Every column is optional so cash and Pix settle without them.
        builder.OwnsOne(e => e.Card, card =>
        {
            card.Property(c => c.Type).HasColumnName("PaymentType").HasMaxLength(20);
            card.Property(c => c.Brand).HasColumnName("CardBrand").HasMaxLength(30);
            card.Property(c => c.Installments).HasColumnName("Installments");
            card.Property(c => c.AuthCode).HasColumnName("AuthCode").HasMaxLength(50);
            card.Property(c => c.Nsu).HasColumnName("Nsu").HasMaxLength(50);
        });
        builder.Navigation(e => e.Card).IsRequired();

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.SaleId);
    }
}
