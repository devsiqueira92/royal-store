using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ClawbackConfiguration : IEntityTypeConfiguration<Clawback>
{
    public void Configure(EntityTypeBuilder<Clawback> builder)
    {
        builder.ToTable("Clawbacks");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.CustomerId).IsRequired();
        builder.Property(e => e.SaleId).IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        // Enforces FR-022f: a sale can never be reversed twice.
        builder.HasIndex(e => new { e.TenantId, e.SaleId }).IsUnique();
    }
}
