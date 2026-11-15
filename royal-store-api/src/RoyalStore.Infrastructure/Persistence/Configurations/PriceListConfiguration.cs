using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> builder)
    {
        builder.ToTable("PriceLists");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(PriceList.NameMaxLength)
            .IsRequired();

        builder.Property(e => e.NormalizedName)
            .HasMaxLength(PriceList.NameMaxLength)
            .IsRequired();

        builder.Property(e => e.IsDefault).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        // Names are unique per store ignoring case and surrounding spaces (061 FR-002).
        builder.HasIndex(e => new { e.TenantId, e.NormalizedName }).IsUnique();

        // At most one default list per store, guaranteed by the database rather than by a
        // read-then-write check, so two concurrent switches cannot both win (061 FR-003).
        builder.HasIndex(e => e.TenantId)
            .IsUnique()
            .HasFilter("\"IsDefault\" = true")
            .HasDatabaseName("IX_PriceLists_TenantId_Default");

        builder.HasIndex(e => new { e.TenantId, e.IsActive });
    }
}
