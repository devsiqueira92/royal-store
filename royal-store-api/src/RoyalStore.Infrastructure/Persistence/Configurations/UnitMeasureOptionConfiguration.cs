using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class UnitMeasureOptionConfiguration : IEntityTypeConfiguration<UnitMeasureOption>
{
    public void Configure(EntityTypeBuilder<UnitMeasureOption> builder)
    {
        builder.ToTable("UnitMeasureOptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Code)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
