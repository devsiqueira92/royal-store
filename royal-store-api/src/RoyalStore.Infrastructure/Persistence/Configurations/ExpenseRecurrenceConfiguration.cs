using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Expenses;
using RoyalStore.Domain.Suppliers;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ExpenseRecurrenceConfiguration : IEntityTypeConfiguration<ExpenseRecurrence>
{
    public void Configure(EntityTypeBuilder<ExpenseRecurrence> builder)
    {
        builder.ToTable("ExpenseRecurrences");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(ExpenseRecurrence.DescriptionMaxLength)
            .IsRequired();

        builder.Property(e => e.PayeeName)
            .HasMaxLength(ExpenseRecurrence.PayeeNameMaxLength);

        builder.Property(e => e.Frequency)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.DueDay).IsRequired();
        builder.Property(e => e.LeadDays).IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.IsVariableAmount).IsRequired();
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.OccurrencesGenerated).IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasOne<ExpenseCategory>()
            .WithMany()
            .HasForeignKey(e => e.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CostCenter>()
            .WithMany()
            .HasForeignKey(e => e.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<FinancialAccount>()
            .WithMany()
            .HasForeignKey(e => e.FinancialAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.TenantId, e.IsActive });
    }
}
