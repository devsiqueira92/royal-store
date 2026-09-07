using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class CashbackRedemptionConfiguration : IEntityTypeConfiguration<CashbackRedemption>
{
    public void Configure(EntityTypeBuilder<CashbackRedemption> builder)
    {
        builder.ToTable("CashbackRedemptions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.CustomerId).IsRequired();
        builder.Property(e => e.SaleId).IsRequired();
        builder.Property(e => e.CashbackEntryId).IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne<CashbackEntry>()
            .WithMany()
            .HasForeignKey(e => e.CashbackEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Reversal looks up every redemption a sale made; balance/history tooling may look up every
        // redemption drawn from one entry.
        builder.HasIndex(e => new { e.TenantId, e.SaleId });
        builder.HasIndex(e => new { e.TenantId, e.CashbackEntryId });
    }
}
