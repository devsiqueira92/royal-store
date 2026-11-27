using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Domain.Tenants;
using RoyalStore.Infrastructure.Persistence.Encryption;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.StoreName)
            .HasMaxLength(100)
            .IsRequired();

        // A store may register with its owner's CPF, so the document is PII: encrypted, globally
        // unique on its blind index. The hash column is nullable only so existing rows can be
        // backfilled after the migration; every saved tenant gets one.
        builder.Property(t => t.Document)
            .IsEncrypted()
            .IsRequired();
        builder.HasBlindIndex(nameof(Tenant.Document), BlindIndexKind.Digits);

        builder.Property(t => t.DocumentType)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();
        builder.Property(t => t.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasIndex(BlindIndex.PropertyName(nameof(Tenant.Document))).IsUnique();
    }
}
