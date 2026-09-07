using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.SaleId).IsRequired();
        builder.Property(e => e.ProductId).IsRequired();
        builder.Property(e => e.VariantId).IsRequired();

        builder.Property(e => e.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ProductCode).HasMaxLength(100);

        builder.Property(e => e.Quantity)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(e => e.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.DiscountItem)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(e => e.TotalPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Catalog snapshot — taken at sale time, never refreshed from the catalog afterwards.
        builder.OwnsOne(e => e.Catalog, catalog =>
        {
            catalog.Property(c => c.VariantLabel).HasColumnName("VariantLabel").HasMaxLength(200).IsRequired();
            catalog.Property(c => c.VariantSize).HasColumnName("VariantSize").HasMaxLength(50);
            catalog.Property(c => c.VariantColor).HasColumnName("VariantColor").HasMaxLength(50);
            catalog.Property(c => c.Sku).HasColumnName("Sku").HasMaxLength(50);
            catalog.Property(c => c.OptionName).HasColumnName("OptionName").HasMaxLength(100);
            catalog.Property(c => c.UnitMeasure).HasColumnName("UnitMeasure").HasMaxLength(36).IsRequired();
        });
        builder.Navigation(e => e.Catalog).IsRequired();

        // Fiscal snapshot — the classification in force for the product on the sale date.
        builder.OwnsOne(e => e.Fiscal, fiscal =>
        {
            fiscal.Property(f => f.Origin).HasColumnName("Origin").HasMaxLength(10).IsRequired();
            fiscal.Property(f => f.Ncm).HasColumnName("Ncm").HasMaxLength(8);
            fiscal.Property(f => f.Cest).HasColumnName("Cest").HasMaxLength(7);
            fiscal.Property(f => f.Cfop).HasColumnName("Cfop").HasMaxLength(10);
            fiscal.Property(f => f.IcmsCst).HasColumnName("IcmsCst").HasMaxLength(10);
            fiscal.Property(f => f.PisCofinsCst).HasColumnName("PisCofinsCst").HasMaxLength(10);
            fiscal.Property(f => f.IvaCst).HasColumnName("IvaCst").HasMaxLength(10);
            fiscal.Property(f => f.Nbs).HasColumnName("Nbs").HasMaxLength(20);
            fiscal.Property(f => f.IssCode).HasColumnName("IssCode").HasMaxLength(20);
            fiscal.Property(f => f.IsSelectiveTax).HasColumnName("IsSelectiveTax").HasDefaultValue(false).IsRequired();
        });
        builder.Navigation(e => e.Fiscal).IsRequired();

        // IBPT tax-burden rates. Nullable throughout: absence must stay distinguishable from zero.
        builder.OwnsOne(e => e.TaxRates, rates =>
        {
            rates.Property(r => r.NationalRate).HasColumnName("NationalRate").HasColumnType("decimal(7,4)");
            rates.Property(r => r.FederalRate).HasColumnName("FederalRate").HasColumnType("decimal(7,4)");
            rates.Property(r => r.ImportedFederalRate).HasColumnName("ImportedFederalRate").HasColumnType("decimal(7,4)");
            rates.Property(r => r.StateRate).HasColumnName("StateRate").HasColumnType("decimal(7,4)");
            rates.Property(r => r.MunicipalRate).HasColumnName("MunicipalRate").HasColumnType("decimal(7,4)");
        });
        builder.Navigation(e => e.TaxRates).IsRequired();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProductVariant>()
            .WithMany()
            .HasForeignKey(e => e.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.SaleId);
    }
}
