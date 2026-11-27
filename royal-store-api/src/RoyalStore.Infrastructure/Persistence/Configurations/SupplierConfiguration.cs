using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Domain.Suppliers;
using RoyalStore.Infrastructure.Persistence.Encryption;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.TenantId).IsRequired();

        builder.Property(s => s.Name)
            .HasMaxLength(150)
            .IsRequired();

        // The document accepts a CPF too (DocumentValidator), so a sole-trader supplier's is PII:
        // encrypted, with uniqueness on its blind index. Phone and street address likewise.
        builder.Property(s => s.Cnpj).IsEncrypted();
        builder.HasBlindIndex(nameof(Supplier.Cnpj), BlindIndexKind.Digits);

        builder.Property(s => s.Phone).IsEncrypted();

        builder.Property(s => s.Address).IsEncrypted();

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.ZipCode)
            .HasMaxLength(10);

        builder.Property(s => s.IsActive).IsRequired();

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();
        builder.Property(s => s.UpdatedBy).HasMaxLength(450).IsRequired();

        var cnpjHash = BlindIndex.PropertyName(nameof(Supplier.Cnpj));
        builder.HasIndex(nameof(Supplier.TenantId), cnpjHash)
            .IsUnique()
            .HasFilter($"\"{cnpjHash}\" IS NOT NULL");

        builder.HasIndex(s => s.TenantId);
    }
}
