using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

/// <summary>One submitted cost line. <paramref name="Id"/> null = a new line; otherwise an existing
/// line of the same list (064 FR-010).</summary>
public sealed record ProductCostItemInput(Guid? Id, string Description, decimal Value);

/// <summary>
/// What <see cref="CostCompositionList.ReplaceItems"/> added to and removed from the aggregate, so the
/// repository can register those children explicitly — same reason as <see cref="ProductPriceChanges"/>:
/// a child with a client-generated key discovered on a tracked graph is mistaken for an existing row.
/// </summary>
public sealed record ProductCostItemChanges(
    IReadOnlyList<ProductCostItem> Added,
    IReadOnlyList<ProductCostItem> Removed)
{
    public static readonly ProductCostItemChanges None = new([], []);
}

/// <summary>
/// One line of a cost composition: free text as typed plus an amount (063, re-parented by 064).
/// Informative only — it never feeds <see cref="Product.BaseCostPrice"/>. Owned by the
/// <see cref="CostCompositionList"/> aggregate: only the list creates, changes or removes it.
/// </summary>
public sealed class ProductCostItem : Entity, IAuditableEntity
{
    public const int DescriptionMaxLength = 80;

    public Guid TenantId { get; private set; }
    public Guid CostCompositionListId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Value { get; private set; }

    /// <summary>0-based position in the last submitted list (064 FR-009).</summary>
    public int SortOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private ProductCostItem() : base(Guid.NewGuid()) { }

    internal ProductCostItem(Guid tenantId, Guid costCompositionListId, string description, decimal value, int sortOrder)
        : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        CostCompositionListId = costCompositionListId;
        Description = description.Trim();
        Value = value;
        SortOrder = sortOrder;
    }

    internal static Result Validate(string? description, decimal value)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(Error.Validation("product.cost_item_description_required",
                "Cost item description is required."));

        if (description.Trim().Length > DescriptionMaxLength)
            return Result.Failure(Error.Validation("product.cost_item_description_too_long",
                $"Cost item description must not exceed {DescriptionMaxLength} characters."));

        if (value < 0)
            return Result.Failure(Error.Validation("product.cost_item_value_negative",
                "Cost item value must be zero or greater."));

        if (value != decimal.Round(value, 2))
            return Result.Failure(Error.Validation("product.cost_item_value_precision",
                "Cost item value must have at most two decimal places."));

        return Result.Success();
    }

    internal void Change(string description, decimal value, int sortOrder)
    {
        Description = description.Trim();
        Value = value;
        SortOrder = sortOrder;
    }
}
