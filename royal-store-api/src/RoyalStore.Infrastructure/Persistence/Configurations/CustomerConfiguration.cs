using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Customers;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(150)
            .IsRequired();

        // Stored normalised to digits only, so a CPF is 11 characters and a CNPJ 14.
        builder.Property(e => e.Document)
            .HasMaxLength(14)
            .IsRequired();

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.TermsAccepted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.LoyaltyTierId)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.TenantId, e.Document }).IsUnique();

        // A tier in use can't be deleted (FR-014, enforced by ILoyaltyTierUsageChecker) — Restrict
        // makes that explicit rather than relying on the usage check alone.
        builder.HasOne(e => e.LoyaltyTier)
            .WithMany()
            .HasForeignKey(e => e.LoyaltyTierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
