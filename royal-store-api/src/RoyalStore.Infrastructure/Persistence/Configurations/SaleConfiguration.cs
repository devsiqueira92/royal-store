using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Customers;
using RoyalStore.Domain.Identity;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();

        // Reserved for a future cash-register-session feature — accepted and stored, never required
        // or validated (038-register-sale-checkout FR-007).
        builder.Property(e => e.CashShiftId);

        builder.Property(e => e.UserId).HasMaxLength(450);

        builder.Property(e => e.Subtotal)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(e => e.DiscountAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(e => e.LoyaltyDiscountApplied)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(e => e.PaymentDiscountApplied)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(e => e.TaxAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(e => e.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.CashbackApplied)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(e => e.CashbackEarned)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(e => e.CashbackRemaining)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(e => e.FulfillmentMode)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.BranchId).HasMaxLength(100);

        builder.Property(e => e.SoldAt).IsRequired();
        builder.Property(e => e.SyncedAt).IsRequired();

        builder.Property(e => e.FiscalStatus)
            .HasConversion<int>()
            .HasDefaultValue(InvoiceStatus.Pending)
            .IsRequired();

        builder.Property(e => e.ExternalInvoiceId);
        builder.Property(e => e.ReceiptPrintUrl);
        builder.Property(e => e.FiscalErrorMessage);

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        // Customer snapshot — frozen at sale time, deliberately excludes document/phone. Nullable:
        // absent on guest sales.
        builder.OwnsOne(e => e.CustomerSnapshot, snapshot =>
        {
            snapshot.Property(s => s.Name).HasColumnName("CustomerName").HasMaxLength(150).IsRequired();
            snapshot.Property(s => s.LoyaltyTierName).HasColumnName("CustomerLoyaltyTierName").HasMaxLength(50).IsRequired();
        });

        // Delivery/pickup address — frozen at sale time, every column nullable. The required
        // navigation (same as SalePayment.Card) makes an all-NULL row load as SaleAddress.Empty, not null.
        builder.OwnsOne(e => e.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(SaleAddress.StreetMaxLength);
            address.Property(a => a.Number).HasColumnName("AddressNumber").HasMaxLength(SaleAddress.NumberMaxLength);
            address.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(SaleAddress.CityMaxLength);
            address.Property(a => a.ZipCode).HasColumnName("AddressZipCode").HasMaxLength(SaleAddress.ZipCodeMaxLength);
        });
        builder.Navigation(e => e.Address).IsRequired();

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Nullable: null means the sale was unidentified. A customer in use is never deleted
        // (customers are deactivated, not erased), so Restrict is never actually exercised — it's
        // here to make that invariant explicit rather than implicit.
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Payments)
            .WithOne()
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // The collections are exposed read-only, so EF must write through the backing fields.
        builder.Navigation(e => e.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(e => e.Payments).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.CashShiftId);
    }
}
