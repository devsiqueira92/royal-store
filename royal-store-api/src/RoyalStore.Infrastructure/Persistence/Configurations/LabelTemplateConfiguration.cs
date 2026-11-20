using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Application.Labels;
using RoyalStore.Domain.Labels;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class LabelTemplateConfiguration : IEntityTypeConfiguration<LabelTemplate>
{
    public void Configure(EntityTypeBuilder<LabelTemplate> builder)
    {
        builder.ToTable("LabelTemplates", table =>
            // An inactive label is never the default (065 FR-009); the domain enforces it, this
            // keeps a row that slipped past it from ever being stored.
            table.HasCheckConstraint("CK_LabelTemplates_Default_Active", "NOT \"IsDefault\" OR \"IsActive\""));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(LabelTemplate.NameMaxLength)
            .IsRequired();

        builder.Property(e => e.NormalizedName)
            .HasMaxLength(LabelTemplate.NameMaxLength)
            .IsRequired();

        // text, not jsonb: jsonb re-orders keys and drops whitespace, and the layout must come back
        // exactly as sent (065 SC-001, research D4).
        builder.Property(e => e.Configuration)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.SourceTemplateId)
            .HasMaxLength(LabelTemplate.SourceTemplateIdMaxLength);

        builder.Property(e => e.IsDefault).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        // One format per template and one template per format (065 FR-021). Restrict: a format is
        // only ever removed together with its template, by the delete handler.
        builder.HasOne(e => e.Format)
            .WithOne()
            .HasForeignKey<LabelTemplate>(e => e.LabelFormatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.LabelFormatId).IsUnique();

        // Names unique per store ignoring case and surrounding spaces (065 FR-004).
        builder.HasIndex(e => new { e.TenantId, e.NormalizedName })
            .IsUnique()
            .HasDatabaseName(LabelIndexNames.Name);

        // At most one default label per store, guaranteed by the database (065 FR-009).
        builder.HasIndex(e => e.TenantId)
            .IsUnique()
            .HasFilter("\"IsDefault\" = true")
            .HasDatabaseName(LabelIndexNames.Default);

        builder.HasIndex(e => new { e.TenantId, e.IsActive });
    }
}
