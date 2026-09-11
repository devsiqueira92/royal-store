using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Identity;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class CashShiftConfiguration : IEntityTypeConfiguration<CashShift>
{
    public void Configure(EntityTypeBuilder<CashShift> builder)
    {
        builder.ToTable("CashShifts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();

        builder.Property(e => e.UserId).HasMaxLength(450);

        builder.Property(e => e.OpenedAt).IsRequired();

        builder.Property(e => e.StartingCash)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.ExpectedEndingCash)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.ActualEndingCash)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // No relationship to Sale is configured here (or on Sale's own configuration): a sale's
        // CashShiftId is reserved for a future cash-register-session feature and is neither required
        // nor validated against a real CashShift (038-register-sale-checkout FR-007) — an enforced
        // FK would contradict that by rejecting exactly the unvalidated values the feature calls for.
        builder.HasMany(e => e.Movements)
            .WithOne()
            .HasForeignKey(m => m.CashShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.TenantId);
    }
}
