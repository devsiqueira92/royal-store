using FluentValidation;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.CreateExpenseCategory;

public sealed class CreateExpenseCategoryValidator : AbstractValidator<CreateExpenseCategoryCommand>
{
    public CreateExpenseCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(ExpenseCategory.NameMaxLength);

        RuleFor(x => x.Color)
            .MaximumLength(ExpenseCategory.ColorMaxLength);
    }
}
