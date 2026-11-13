using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ToTable("ExpenseCategories");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(ExpenseCategory.NameMaxLength)
            .IsRequired();

        builder.Property(e => e.Color).HasMaxLength(ExpenseCategory.ColorMaxLength);

        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.DisplayOrder).IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        // Self-referencing hierarchy. Restrict, not Cascade: deleting a parent must never silently
        // take its children (and their expenses' classification) with it — FR-027 refuses the
        // delete outright while the category is referenced.
        builder.HasOne<ExpenseCategory>()
            .WithMany()
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.TenantId, e.ParentId });
        builder.HasIndex(e => new { e.TenantId, e.Name, e.ParentId }).IsUnique();
    }
}
