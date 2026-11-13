using FluentValidation;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.UpdateExpenseCategory;

public sealed class UpdateExpenseCategoryValidator : AbstractValidator<UpdateExpenseCategoryCommand>
{
    public UpdateExpenseCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(ExpenseCategory.NameMaxLength);

        RuleFor(x => x.Color).MaximumLength(ExpenseCategory.ColorMaxLength);
    }
}
