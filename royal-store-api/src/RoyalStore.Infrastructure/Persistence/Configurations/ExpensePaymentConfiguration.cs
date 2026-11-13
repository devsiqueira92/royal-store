using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ExpensePaymentConfiguration : IEntityTypeConfiguration<ExpensePayment>
{
    public void Configure(EntityTypeBuilder<ExpensePayment> builder)
    {
        builder.ToTable("ExpensePayments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ExpenseId).IsRequired();
        builder.Property(e => e.FinancialAccountId).IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.PaidAt).IsRequired();

        builder.Property(e => e.MethodNote).HasMaxLength(ExpensePayment.MethodNoteMaxLength);
        builder.Property(e => e.Notes).HasMaxLength(ExpensePayment.NotesMaxLength);

        builder.Property(e => e.IsReversed).IsRequired();
        builder.Property(e => e.ReversalReason).HasMaxLength(ExpensePayment.ReversalReasonMaxLength);

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        // Restrict: an account that money has actually left cannot be deleted out from under its
        // payment history (FR-027).
        builder.HasOne<FinancialAccount>()
            .WithMany()
            .HasForeignKey(e => e.FinancialAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.ExpenseId);

        // Serves the per-account outflow report, which replaces the stored balance FR-026a forbids.
        builder.HasIndex(e => new { e.TenantId, e.FinancialAccountId, e.PaidAt });
    }
}
