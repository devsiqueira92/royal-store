using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// Ships the table only. No file is ever written here by feature 060 — the storage integration is a
/// later feature (FR-050), and <c>StorageReference</c> is deliberately provider-agnostic.
/// </summary>
public sealed class ExpenseAttachmentConfiguration : IEntityTypeConfiguration<ExpenseAttachment>
{
    public void Configure(EntityTypeBuilder<ExpenseAttachment> builder)
    {
        builder.ToTable("ExpenseAttachments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ExpenseId).IsRequired();

        builder.Property(e => e.FileName)
            .HasMaxLength(ExpenseAttachment.FileNameMaxLength)
            .IsRequired();

        builder.Property(e => e.ContentType)
            .HasMaxLength(ExpenseAttachment.ContentTypeMaxLength)
            .IsRequired();

        builder.Property(e => e.SizeInBytes).IsRequired();

        builder.Property(e => e.StorageReference)
            .HasMaxLength(ExpenseAttachment.StorageReferenceMaxLength)
            .IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasIndex(e => e.ExpenseId);
    }
}
