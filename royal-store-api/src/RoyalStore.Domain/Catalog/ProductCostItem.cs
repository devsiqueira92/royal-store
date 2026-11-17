using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

/// <summary>One submitted cost line. <paramref name="Id"/> null = a new line; otherwise an existing
/// line of the same product (063 FR-004).</summary>
public sealed record ProductCostItemInput(Guid? Id, string Description, decimal Value);

/// <summary>
/// What <see cref="Product.ApplyCostItems"/> added to and removed from the aggregate, so the
/// repository can register those children explicitly — same reason as <see cref="ProductPriceChanges"/>:
/// a child with a client-generated key discovered on a tracked graph is mistaken for an existing row.
/// </summary>
public sealed record ProductCostItemChanges(
    IReadOnlyList<ProductCostItem> Added,
    IReadOnlyList<ProductCostItem> Removed)
{
    public static readonly ProductCostItemChanges None = new([], []);
}

/// <summary>Every child change of one product update, for explicit registration by the handler.</summary>
public sealed record ProductChildChanges(ProductPriceChanges Prices, ProductCostItemChanges CostItems);

/// <summary>
/// One line of a product's cost breakdown: free text as typed plus an amount (063). Informative only —
/// it never feeds <see cref="Product.BaseCostPrice"/> (FR-015). Owned by the <see cref="Product"/>
/// aggregate: only the product creates, changes or removes it.
/// </summary>
public sealed class ProductCostItem : Entity, IAuditableEntity
{
    public const int DescriptionMaxLength = 80;

    public Guid TenantId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Value { get; private set; }

    /// <summary>0-based position in the last submitted breakdown (FR-007).</summary>
    public int SortOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private ProductCostItem() : base(Guid.NewGuid()) { }

    internal ProductCostItem(Guid tenantId, Guid productId, string description, decimal value, int sortOrder)
        : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        ProductId = productId;
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
