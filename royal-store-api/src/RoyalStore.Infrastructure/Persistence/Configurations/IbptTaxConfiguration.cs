using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Fiscal;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class IbptTaxConfiguration : IEntityTypeConfiguration<IbptTax>
{
    public void Configure(EntityTypeBuilder<IbptTax> builder)
    {
        builder.ToTable("IbptTaxes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Exception)
            .HasMaxLength(10);

        builder.Property(x => x.Type).IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.NationalRate).HasColumnType("decimal(5,2)").IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.FederalRate).HasColumnType("decimal(5,2)").IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.ImportedFederalRate).HasColumnType("decimal(5,2)").IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.StateRate).HasColumnType("decimal(5,2)").IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.MunicipalRate).HasColumnType("decimal(5,2)").IsRequired().HasDefaultValue(0m);

        builder.Property(x => x.EffectiveStart).HasColumnType("date").IsRequired();
        builder.Property(x => x.EffectiveEnd).HasColumnType("date").IsRequired();

        builder.Property(x => x.Key).HasMaxLength(50);
        builder.Property(x => x.Version).HasMaxLength(20);
        builder.Property(x => x.Source).HasMaxLength(200);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Persistence-only normalized search columns (accent- and punctuation-insensitive).
        // Populated by the schema-bound scalar function created in the AddIbptTaxes migration.
        builder.Property<string>("NormalizedCode")
            .HasMaxLength(4000)
            .HasComputedColumnSql("[dbo].[fn_NormalizeSearchText]([Code])", stored: true);

        builder.Property<string>("NormalizedText")
            .HasMaxLength(4000)
            .HasComputedColumnSql("[dbo].[fn_NormalizeSearchText]([Description])", stored: true);

        builder.HasIndex("NormalizedCode");
        builder.HasIndex("NormalizedText");
        builder.HasIndex(x => new { x.Code, x.EffectiveStart });
    }
}
