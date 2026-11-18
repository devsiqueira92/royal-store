using FluentValidation;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.CostCompositions;

/// <summary>
/// Request-shape rules for the lines of a cost composition list, shared by the create and update
/// validators (064; moved here from the product commands, where 063 had them). The aggregate
/// re-checks the same invariants and owns the id-ownership rule.
/// </summary>
public static class CostCompositionInputs
{
    public static void ValidateShape<T>(
        IReadOnlyList<ProductCostItemInput>? items, ValidationContext<T> context)
    {
        // An empty list is a valid list (064 FR-007).
        if (items is null)
            return;

        if (items.Count > CostCompositionList.MaxItems)
            context.AddFailure("Items", $"A cost composition can have at most {CostCompositionList.MaxItems} items.");

        if (items.Where(i => i.Id.HasValue).GroupBy(i => i.Id).Any(g => g.Count() > 1))
            context.AddFailure("Items", "Each cost item may appear only once.");

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            if (string.IsNullOrWhiteSpace(item.Description))
                context.AddFailure($"Items[{i}].Description", "Description is required.");
            else if (item.Description.Trim().Length > ProductCostItem.DescriptionMaxLength)
                context.AddFailure($"Items[{i}].Description",
                    $"Description must not exceed {ProductCostItem.DescriptionMaxLength} characters.");

            if (item.Value < 0)
                context.AddFailure($"Items[{i}].Value", "Value must be zero or greater.");
            else if (item.Value != decimal.Round(item.Value, 2))
                context.AddFailure($"Items[{i}].Value", "Value must have at most two decimal places.");
        }
    }
}
