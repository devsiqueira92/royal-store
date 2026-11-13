using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Expenses;
using RoyalStore.Domain.Suppliers;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(Expense.DescriptionMaxLength)
            .IsRequired();

        builder.Property(e => e.PayeeName)
            .HasMaxLength(Expense.PayeeNameMaxLength);

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.IsEstimated).IsRequired();
        builder.Property(e => e.CompetenceMonth).IsRequired();
        builder.Property(e => e.DueDate).IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Notes).HasMaxLength(Expense.NotesMaxLength);

        builder.Property(e => e.RecurrenceCycleKey)
            .HasMaxLength(Expense.RecurrenceCycleKeyMaxLength);

        // Bumped by every state transition so two payments racing each other cannot both commit
        // against a stale balance (FR-019). Same mechanism as CashShift.Version.
        builder.Property(e => e.Version)
            .IsRequired()
            .IsConcurrencyToken();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        // The aggregate owns these two: deleting the expense takes them with it.
        builder.HasMany(e => e.Payments)
            .WithOne()
            .HasForeignKey(p => p.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Attachments)
            .WithOne()
            .HasForeignKey(a => a.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Payments).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(e => e.Attachments).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Restrict on every classification FK — this is what makes FR-027 ("cannot delete something
        // an expense still references") a database guarantee and not only a handler check.
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

        builder.HasOne<ExpenseRecurrence>()
            .WithMany()
            .HasForeignKey(e => e.ExpenseRecurrenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.TenantId, e.CompetenceMonth });
        builder.HasIndex(e => new { e.TenantId, e.DueDate });
        builder.HasIndex(e => new { e.TenantId, e.Status });
        builder.HasIndex(e => new { e.TenantId, e.ExpenseCategoryId });

        // THE idempotency guarantee (FR-032, research Decision 2). The generation handler's
        // DbUpdateException catch is meaningless without this index: a pre-check alone loses the
        // race between check and insert, so the database is the only place uniqueness survives two
        // concurrent runs. Filtered, because hand-entered expenses carry no recurrence.
        builder.HasIndex(e => new { e.TenantId, e.ExpenseRecurrenceId, e.RecurrenceCycleKey })
            .IsUnique()
            .HasFilter("\"ExpenseRecurrenceId\" IS NOT NULL");
    }
}
