using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class CashbackEntryConfiguration : IEntityTypeConfiguration<CashbackEntry>
{
    public void Configure(EntityTypeBuilder<CashbackEntry> builder)
    {
        builder.ToTable("CashbackEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.CustomerId).IsRequired();
        builder.Property(e => e.SaleId).IsRequired();

        builder.Property(e => e.GrantedAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.RemainingAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.EarnedAt).IsRequired();
        builder.Property(e => e.ExpiresAt).IsRequired();

        builder.Property(e => e.State)
            .HasConversion<int>()
            .IsRequired();

        // Optimistic concurrency token — mirrors Product.Version (research.md Decision 5).
        builder.Property(e => e.Version)
            .IsRequired()
            .IsConcurrencyToken();

        builder.HasIndex(e => new { e.TenantId, e.CustomerId, e.State, e.ExpiresAt });
    }
}
