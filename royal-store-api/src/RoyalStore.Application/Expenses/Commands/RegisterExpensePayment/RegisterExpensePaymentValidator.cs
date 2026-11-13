using FluentValidation;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.RegisterExpensePayment;

public sealed class RegisterExpensePaymentValidator : AbstractValidator<RegisterExpensePaymentCommand>
{
    public RegisterExpensePaymentValidator()
    {
        RuleFor(x => x.ExpenseId).NotEmpty();

        RuleFor(x => x.FinancialAccountId)
            .NotEmpty().WithMessage("Financial account is required to register a payment.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.")
            .PrecisionScale(18, 2, true)
            .WithMessage("Payment amount must have at most two decimal places.");

        // The balance check itself is a domain rule on Expense.RegisterPayment, not a validator
        // concern — it needs the loaded aggregate.

        RuleFor(x => x.MethodNote).MaximumLength(ExpensePayment.MethodNoteMaxLength);
        RuleFor(x => x.Notes).MaximumLength(ExpensePayment.NotesMaxLength);
    }
}
