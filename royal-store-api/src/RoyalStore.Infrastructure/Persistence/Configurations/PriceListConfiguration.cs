using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> builder)
    {
        builder.ToTable("PriceLists", table =>
        {
            // The percentage rule (062) is all-or-nothing, and a list is never its own base. The
            // domain enforces both; these keep a row that slipped past it from ever being stored.
            table.HasCheckConstraint("CK_PriceLists_Rule_Complete",
                "(\"DiscountPercentage\" IS NULL) = (\"BasePriceListId\" IS NULL)");
            table.HasCheckConstraint("CK_PriceLists_Rule_NotSelf",
                "\"BasePriceListId\" <> \"Id\"");
        });

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

        // −100.00 … +999.99. Negative = discount, positive = surcharge.
        builder.Property(e => e.DiscountPercentage).HasPrecision(5, 2);

        // Restrict: a list other lists are calculated from cannot be deleted from under them
        // (062 FR-009). No navigation — every use already has the store's lists in memory.
        builder.HasOne<PriceList>()
            .WithMany()
            .HasForeignKey(e => e.BasePriceListId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.BasePriceListId);
    }
}
