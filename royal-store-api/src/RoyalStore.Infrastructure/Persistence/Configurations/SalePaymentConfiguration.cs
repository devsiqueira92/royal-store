using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Payments;
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

        builder.Property(e => e.FiscalType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Which catalog Payment (037-payment-methods-catalog) this line settles against — always
        // required now; every checkout line references a resolved, active, tenant-owned entry. Never
        // deleted alongside it, since Payment rows are themselves never deleted (FR-004 of 037);
        // Restrict just documents that intent explicitly.
        builder.Property(e => e.PaymentId).IsRequired();
        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(e => e.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Card detail. Every column is optional so cash, Pix, and cashback settlements record
        // without them.
        builder.OwnsOne(e => e.Card, card =>
        {
            card.Property(c => c.Flag).HasColumnName("CardFlag").HasMaxLength(30);
            card.Property(c => c.Type).HasColumnName("PaymentType").HasMaxLength(20);
            card.Property(c => c.Installments).HasColumnName("Installments");
        });
        builder.Navigation(e => e.Card).IsRequired();

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.SaleId);
    }
}
