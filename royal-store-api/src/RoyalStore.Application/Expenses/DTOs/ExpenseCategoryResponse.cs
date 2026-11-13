using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>
/// A category and everything beneath it. The tree is assembled in memory from a single flat read
/// (research Decision 5).
/// </summary>
public record ExpenseCategoryResponse(
    Guid Id,
    string Name,
    Guid? ParentId,
    string? Color,
    bool IsActive,
    int DisplayOrder,
    IReadOnlyList<ExpenseCategoryResponse> Children)
{
    public static ExpenseCategoryResponse From(
        ExpenseCategory category, IReadOnlyList<ExpenseCategoryResponse> children) =>
        new(category.Id,
            category.Name,
            category.ParentId,
            category.Color,
            category.IsActive,
            category.DisplayOrder,
            children);

    /// <summary>Builds the forest, rooted at the categories with no parent.</summary>
    public static IReadOnlyList<ExpenseCategoryResponse> BuildTree(
        IReadOnlyCollection<ExpenseCategory> categories)
    {
        var childrenByParent = categories
            .Where(c => c.ParentId is not null)
            .GroupBy(c => c.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        return categories
            .Where(c => c.ParentId is null)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => BuildNode(c, childrenByParent, 0))
            .ToList();
    }

    /// <summary>
    /// <paramref name="depth"/> guards against a cycle in stored data: the domain refuses to create
    /// one, but a report must not hang if one ever exists.
    /// </summary>
    private static ExpenseCategoryResponse BuildNode(
        ExpenseCategory category,
        IReadOnlyDictionary<Guid, List<ExpenseCategory>> childrenByParent,
        int depth)
    {
        if (depth > 16 || !childrenByParent.TryGetValue(category.Id, out var children))
            return From(category, []);

        return From(
            category,
            [.. children
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => BuildNode(c, childrenByParent, depth + 1))]);
    }
}
