using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Labels;
using RoyalStore.Infrastructure.Seed;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class LabelFormatConfiguration : IEntityTypeConfiguration<LabelFormat>
{
    public void Configure(EntityTypeBuilder<LabelFormat> builder)
    {
        builder.ToTable("LabelFormats", table =>
        {
            // The domain enforces every measurement rule (065 FR-002); these keep a row that slipped
            // past it from ever being stored.
            table.HasCheckConstraint("CK_LabelFormats_Sizes_Positive",
                "\"PaperWidth\" > 0 AND \"PaperHeight\" > 0 AND \"LabelWidth\" > 0 AND \"LabelHeight\" > 0");
            table.HasCheckConstraint("CK_LabelFormats_Columns_Range",
                $"\"Columns\" BETWEEN {LabelFormat.MinColumns} AND {LabelFormat.MaxColumns}");
            table.HasCheckConstraint("CK_LabelFormats_Offsets_NonNegative",
                "\"HorizontalSpacing\" >= 0 AND \"VerticalSpacing\" >= 0 AND \"MarginTop\" >= 0 " +
                "AND \"MarginRight\" >= 0 AND \"MarginBottom\" >= 0 AND \"MarginLeft\" >= 0");

            // A format is a system one exactly when it belongs to no store (066 research D1).
            table.HasCheckConstraint("CK_LabelFormats_System_Tenant",
                "\"IsSystem\" = (\"TenantId\" = '00000000-0000-0000-0000-000000000000')");
        });

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(LabelFormat.NameMaxLength)
            .IsRequired();

        builder.Property(e => e.PaperType).IsRequired();

        // Millimetres, two decimals; the domain caps them at 1,000.
        builder.Property(e => e.PaperWidth).HasPrecision(6, 2);
        builder.Property(e => e.PaperHeight).HasPrecision(6, 2);
        builder.Property(e => e.LabelWidth).HasPrecision(6, 2);
        builder.Property(e => e.LabelHeight).HasPrecision(6, 2);
        builder.Property(e => e.HorizontalSpacing).HasPrecision(6, 2);
        builder.Property(e => e.VerticalSpacing).HasPrecision(6, 2);
        builder.Property(e => e.MarginTop).HasPrecision(6, 2);
        builder.Property(e => e.MarginRight).HasPrecision(6, 2);
        builder.Property(e => e.MarginBottom).HasPrecision(6, 2);
        builder.Property(e => e.MarginLeft).HasPrecision(6, 2);

        builder.Property(e => e.Columns).IsRequired();

        builder.Property(e => e.IsSystem).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasIndex(e => e.TenantId);

        // The system configurations (066 FR-009), inserted by migration.
        builder.HasData(SystemLabelConfigurations.Formats);
    }
}
