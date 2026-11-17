using FluentValidation;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products;

/// <summary>
/// Request-shape rules for a product's cost breakdown, shared by the create and edit validators
/// (063). The aggregate re-checks the same invariants and owns the id-ownership rule.
/// </summary>
public static class ProductCostItemInputs
{
    public static void ValidateShape<T>(
        IReadOnlyList<ProductCostItemInput>? items, ValidationContext<T> context)
    {
        // Absent = the stored breakdown is left untouched (063 FR-005).
        if (items is null)
            return;

        if (items.Count > Product.MaxCostItems)
            context.AddFailure("CostItems", $"A product can have at most {Product.MaxCostItems} cost items.");

        if (items.Where(i => i.Id.HasValue).GroupBy(i => i.Id).Any(g => g.Count() > 1))
            context.AddFailure("CostItems", "Each cost item may appear only once.");

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            if (string.IsNullOrWhiteSpace(item.Description))
                context.AddFailure($"CostItems[{i}].Description", "Description is required.");
            else if (item.Description.Trim().Length > ProductCostItem.DescriptionMaxLength)
                context.AddFailure($"CostItems[{i}].Description",
                    $"Description must not exceed {ProductCostItem.DescriptionMaxLength} characters.");

            if (item.Value < 0)
                context.AddFailure($"CostItems[{i}].Value", "Value must be zero or greater.");
            else if (item.Value != decimal.Round(item.Value, 2))
                context.AddFailure($"CostItems[{i}].Value", "Value must have at most two decimal places.");
        }
    }
}
