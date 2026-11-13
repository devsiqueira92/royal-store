using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>The category as it appears inline on an expense — enough to label and colour a row.</summary>
public record ExpenseCategorySummary(Guid Id, string Name, string? Color)
{
    public static ExpenseCategorySummary From(ExpenseCategory category) =>
        new(category.Id, category.Name, category.Color);
}
