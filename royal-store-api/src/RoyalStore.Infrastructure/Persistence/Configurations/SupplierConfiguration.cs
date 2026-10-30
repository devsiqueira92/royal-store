using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Suppliers;

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

        builder.Property(s => s.Cnpj)
            .HasMaxLength(14);

        builder.Property(s => s.Phone)
            .HasMaxLength(20);

        builder.Property(s => s.Address)
            .HasMaxLength(200);

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.ZipCode)
            .HasMaxLength(10);

        builder.Property(s => s.IsActive).IsRequired();

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();
        builder.Property(s => s.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasIndex(s => new { s.TenantId, s.Cnpj })
            .IsUnique()
            .HasFilter("[Cnpj] IS NOT NULL");

        builder.HasIndex(s => s.TenantId);
    }
}
