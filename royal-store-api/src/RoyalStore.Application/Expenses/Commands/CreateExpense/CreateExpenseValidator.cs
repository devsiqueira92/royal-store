using FluentValidation;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.CreateExpense;

public sealed class CreateExpenseValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Expense description is required.")
            .MaximumLength(Expense.DescriptionMaxLength);

        // Optional — only its length is checked.
        RuleFor(x => x.PayeeName)
            .MaximumLength(Expense.PayeeNameMaxLength);

        RuleFor(x => x.ExpenseCategoryId)
            .NotEmpty().WithMessage("Expense category is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Expense amount must be greater than zero.")
            .PrecisionScale(18, 2, true)
            .WithMessage("Expense amount must have at most two decimal places.");

        RuleFor(x => x.Notes)
            .MaximumLength(Expense.NotesMaxLength);
    }
}
