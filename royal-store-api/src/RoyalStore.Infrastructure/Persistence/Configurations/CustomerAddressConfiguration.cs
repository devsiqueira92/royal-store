using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Customers;
using RoyalStore.Infrastructure.Persistence.Encryption;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddresses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.CustomerId).IsRequired();

        // Every address field is optional (045 clarification). ZipCode is stored digits-only and
        // State as the upper-case UF, so their lengths are exact rather than maximums. Zip code,
        // neighborhood, city and state stay plaintext for routing/reporting; the fields that pin
        // down the household are encrypted (the domain enforces their plaintext limits).
        builder.Property(e => e.ZipCode).HasMaxLength(PostalAddress.ZipCodeLength);
        builder.Property(e => e.Street).IsEncrypted();
        builder.Property(e => e.Number).IsEncrypted();
        builder.Property(e => e.Neighborhood).HasMaxLength(PostalAddress.NeighborhoodMaxLength);
        builder.Property(e => e.City).HasMaxLength(PostalAddress.CityMaxLength);
        builder.Property(e => e.State).HasMaxLength(PostalAddress.StateLength);
        builder.Property(e => e.Complement).IsEncrypted();
        builder.Property(e => e.AdditionalInfo).IsEncrypted();

        builder.Property(e => e.IsDefaultDelivery)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.CustomerId);

        // The relationship to Customer is configured on the Customer side (CustomerConfiguration).
    }
}
