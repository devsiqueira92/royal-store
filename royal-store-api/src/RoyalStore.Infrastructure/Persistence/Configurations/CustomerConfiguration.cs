using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Domain.Customers;
using RoyalStore.Infrastructure.Persistence.Encryption;

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

        // PII, encrypted at rest (text columns; the domain enforces the plaintext limits). Document
        // is normalised to digits (CPF 11 / CNPJ 14) and optional; lookups and the per-tenant
        // uniqueness rule go through its blind index, never the ciphertext.
        builder.Property(e => e.Document).IsEncrypted();
        builder.HasBlindIndex(nameof(Customer.Document), BlindIndexKind.Digits);

        // Blind-indexed (digits only) so the checkout can find a customer by phone; not unique —
        // relatives often share one.
        builder.Property(e => e.Phone).IsEncrypted();
        builder.HasBlindIndex(nameof(Customer.Phone), BlindIndexKind.Digits);

        builder.Property(e => e.BirthDate).IsEncrypted();

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
        builder.HasIndex(nameof(Customer.TenantId), BlindIndex.PropertyName(nameof(Customer.Document))).IsUnique();
        builder.HasIndex(nameof(Customer.TenantId), BlindIndex.PropertyName(nameof(Customer.Phone)));

        // Bumped by every address change; protects the one-default rule across concurrent requests
        // (045 research Decision 4).
        builder.Property(e => e.Version)
            .IsRequired()
            .IsConcurrencyToken();

        // Cascade is never actually exercised — customers are deactivated, not deleted — it just
        // states that addresses are owned by their customer.
        builder.HasMany(e => e.Addresses)
            .WithOne()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // The collection is exposed read-only, so EF must write through the backing field.
        builder.Navigation(e => e.Addresses).UsePropertyAccessMode(PropertyAccessMode.Field);

        // A tier in use can't be deleted (FR-014, enforced by ILoyaltyTierUsageChecker) — Restrict
        // makes that explicit rather than relying on the usage check alone.
        builder.HasOne(e => e.LoyaltyTier)
            .WithMany()
            .HasForeignKey(e => e.LoyaltyTierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
