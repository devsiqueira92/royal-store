using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Tenants;

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

        builder.Property(t => t.Document)
            .HasMaxLength(14)
            .IsRequired();

        builder.Property(t => t.DocumentType)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();
        builder.Property(t => t.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasIndex(t => t.Document).IsUnique();
    }
}
