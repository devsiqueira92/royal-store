using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

/// <summary>
/// A product's cost breakdown as a list of its own (064): free-text cost lines with an amount,
/// created before the product exists and tied to it by a product save that names the list.
/// <para>
/// A list belongs to one store and is reusable: any number of the store's products may use it. The
/// link lives on the product (<see cref="Product.CostCompositionId"/>), so the list knows nothing
/// about who uses it.
/// </para>
/// <para>
/// Informative only: it never feeds <see cref="Product.BaseCostPrice"/>.
/// </para>
/// </summary>
public sealed class CostCompositionList : Entity, IAuditableEntity
{
    public const int MaxItems = 50;
    public const int NameMaxLength = 100;

    public Guid TenantId { get; private set; }

    /// <summary>What the manager calls this list. Required, trimmed, at most <see cref="NameMaxLength"/>
    /// characters; names are not required to be unique.</summary>
    public string Name { get; private set; } = string.Empty;

    public ICollection<ProductCostItem> Items { get; private set; } = [];

    /// <summary>
    /// The sum of the amounts of <see cref="Items"/>. Stored, so reading a list never has to add its
    /// lines up. It is only as current as the last <see cref="RecalculateTotal"/>: the create and update
    /// handlers call it once the lines are final. Informative like the lines — it never feeds
    /// <see cref="Product.BaseCostPrice"/>.
    /// </summary>
    public decimal TotalValue { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private CostCompositionList() : base(Guid.NewGuid()) { }

    private CostCompositionList(Guid tenantId) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Creates a list used by no product yet. All lines are new: a line id sent here refers to nothing,
    /// so it fails as not found, exactly as in <see cref="ReplaceItems"/>.
    /// </summary>
    public static Result<CostCompositionList> Create(
        Guid tenantId, string name, IReadOnlyList<ProductCostItemInput> items)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("costcomposition.tenant_required", "Tenant identifier is required.");

        var list = new CostCompositionList(tenantId);

        var named = list.Rename(name);
        if (named.IsFailure)
            return named.Error;

        var replaced = list.ReplaceItems(items);
        if (replaced.IsFailure)
            return replaced.Error;

        return list;
    }

    /// <summary>Gives the list a new name (064 FR-027). Does not touch the lines or the link.</summary>
    public Result Rename(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("costcomposition.name_required", "Cost composition name is required."));

        name = name.Trim();
        if (name.Length > NameMaxLength)
            return Result.Failure(Error.Validation("costcomposition.name_too_long",
                $"Cost composition name must not exceed {NameMaxLength} characters."));

        Name = name;
        return Result.Success();
    }

    /// <summary>
    /// Replaces this list's lines with the submitted set (064 FR-010): lines with an id are updated,
    /// lines without one are added, stored lines left out are removed, and the order sent is the order
    /// kept (FR-009). Nothing changes unless every line is valid (FR-008). Zero lines is allowed (FR-007).
    /// Products using the list see the new lines at once.
    /// </summary>
    public Result<ProductCostItemChanges> ReplaceItems(IReadOnlyList<ProductCostItemInput> items)
    {
        var validation = ValidateItems(items);
        if (validation.IsFailure)
            return validation.Error;

        var added = new List<ProductCostItem>();
        var removed = new List<ProductCostItem>();

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var stored = item.Id is { } id ? Items.First(c => c.Id == id) : null;
            if (stored is not null)
            {
                stored.Change(item.Description, item.Value, i);
                continue;
            }

            var created = new ProductCostItem(TenantId, Id, item.Description, item.Value, i);
            Items.Add(created);
            added.Add(created);
        }

        var submitted = items.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();
        foreach (var stored in Items.ToList())
        {
            if (submitted.Contains(stored.Id) || added.Contains(stored))
                continue;

            Items.Remove(stored);
            removed.Add(stored);
        }

        return new ProductCostItemChanges(added, removed);
    }

    private Result ValidateItems(IReadOnlyList<ProductCostItemInput> items)
    {
        if (items.Count > MaxItems)
            return Result.Failure(Error.Validation("costcomposition.too_many_items",
                $"A cost composition can have at most {MaxItems} items."));

        var seenIds = new HashSet<Guid>();
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            var itemValidation = ProductCostItem.Validate(item.Description, item.Value);
            if (itemValidation.IsFailure)
                return Result.Failure(Product.AttributeError(itemValidation.Error, $"Cost item {i + 1}"));

            if (item.Id is not { } id)
                continue;

            if (!seenIds.Add(id))
                return Result.Failure(Error.Validation("costcomposition.item_duplicate",
                    $"Cost item {i + 1}: each cost item may appear only once."));

            // Lines of another list or another store are never in this collection.
            if (Items.All(c => c.Id != id))
                return Result.Failure(Error.NotFound("costcomposition.item_not_found",
                    $"Cost item {i + 1}: cost item not found in this cost composition."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Sets <see cref="TotalValue"/> to the sum of the lines' amounts. Amounts have at most two
    /// decimals, so the sum is exact and needs no rounding.
    /// </summary>
    public void RecalculateTotal() => TotalValue = Items.Sum(i => i.Value);
}
