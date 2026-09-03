using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class TaxProfileTemplateConfiguration : IEntityTypeConfiguration<TaxProfileTemplate>
{
    public void Configure(EntityTypeBuilder<TaxProfileTemplate> builder)
    {
        builder.ToTable("TaxProfileTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TenantId).IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.TargetItemType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.DefaultCfop).HasMaxLength(4);
        builder.Property(t => t.IcmsCst).HasMaxLength(4);
        builder.Property(t => t.PisCofinsCst).HasMaxLength(4);
        builder.Property(t => t.IvaCst).HasMaxLength(4);

        builder.Property(t => t.IsSelectiveTax)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();
        builder.Property(t => t.UpdatedBy).HasMaxLength(450).IsRequired();

        // The Product -> TaxProfileTemplate relationship is configured from the Product side
        // (ProductConfiguration.cs) via HasOne(p => p.TaxProfileTemplate).WithMany(t => t.Products);
        // configuring it again here would just duplicate the same relationship.

        builder.HasIndex(t => new { t.TenantId, t.TargetItemType });
    }
}
