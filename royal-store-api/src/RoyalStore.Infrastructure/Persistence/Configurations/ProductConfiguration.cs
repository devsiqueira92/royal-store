using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId).IsRequired();

        builder.Property(p => p.CategoryId).IsRequired();

        builder.Property(p => p.SupplierId);

        builder.Property(p => p.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(p => p.Sku)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.IsActive).IsRequired();

        builder.Property(p => p.UnitOfMeasure)
            .HasMaxLength(36);

        builder.Property(p => p.Type)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(ItemType.Product);

        builder.Property(p => p.StockTrack)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.BasePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.BaseCostPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Ncm)
            .HasMaxLength(8);

        builder.Property(p => p.Cest)
            .HasMaxLength(7);

        builder.Property(p => p.Origin)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Nbs).HasMaxLength(9);
        builder.Property(p => p.IssCode).HasMaxLength(20);
        builder.Property(p => p.DefaultCfop).HasMaxLength(4);
        builder.Property(p => p.IcmsCst).HasMaxLength(4);
        builder.Property(p => p.PisCofinsCst).HasMaxLength(4);
        builder.Property(p => p.IvaCst).HasMaxLength(4);

        builder.Property(p => p.IsSelectiveTax);

        builder.Property(p => p.TaxProfileTemplateId).IsRequired();

        builder.Property(p => p.OverrideTaxProfile)
            .IsRequired()
            .HasDefaultValue(false);

        builder.OwnsOne(p => p.Measure, measure =>
        {
            measure.Property(m => m.NetWeight).HasColumnName("NetWeight").HasPrecision(9, 3);
            measure.Property(m => m.GrossWeight).HasColumnName("GrossWeight").HasPrecision(9, 3);
            measure.Property(m => m.Width).HasColumnName("Width").HasPrecision(9, 2);
            measure.Property(m => m.Height).HasColumnName("Height").HasPrecision(9, 2);
            measure.Property(m => m.Length).HasColumnName("Length").HasPrecision(9, 2);
        });
        builder.Navigation(p => p.Measure).IsRequired();

        builder.Property(p => p.Version)
            .IsRequired()
            .IsConcurrencyToken();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
        builder.Property(p => p.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.TaxProfileTemplate)
            .WithMany(t => t.Products)
            .HasForeignKey(p => p.TaxProfileTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Variants)
            .WithOne()
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Photos)
            .WithOne()
            .HasForeignKey(ph => ph.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.TenantId, p.CategoryId });

        builder.HasIndex(p => new { p.TenantId, p.Sku }).IsUnique();

        // Persistence-only normalized search columns (accent- and punctuation-insensitive).
        // Populated by the fn_normalize_search_text function created in the InitialCreate migration.
        builder.Property<string>("NormalizedName")
            .HasMaxLength(4000)
            .HasComputedColumnSql("fn_normalize_search_text(\"Name\")", stored: true);

        builder.Property<string>("NormalizedSku")
            .HasMaxLength(4000)
            .HasComputedColumnSql("fn_normalize_search_text(\"Sku\")", stored: true);

        builder.HasIndex("NormalizedName");
        builder.HasIndex("NormalizedSku");
    }
}
