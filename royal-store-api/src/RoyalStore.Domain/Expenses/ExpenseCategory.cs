using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Expenses;

/// <summary>
/// A named bucket of operating cost, optionally nested under a parent to form the hierarchy the
/// management P&amp;L rolls up (FR-022, FR-023).
/// </summary>
public sealed class ExpenseCategory : Entity, IAuditableEntity
{
    public const int NameMaxLength = 100;
    public const int ColorMaxLength = 7;

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid? ParentId { get; private set; }
    public string? Color { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private ExpenseCategory() : base(Guid.NewGuid()) { }

    private ExpenseCategory(Guid id, Guid tenantId, string name)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        IsActive = true;
    }

    private static Result<(string Name, string? Color)> ValidateFields(string name, string? color)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("expensecategory.name_required", "Category name is required.");

        name = name.Trim();
        if (name.Length > NameMaxLength)
            return Error.Validation("expensecategory.name_too_long",
                $"Category name must not exceed {NameMaxLength} characters.");

        if (!string.IsNullOrWhiteSpace(color))
        {
            color = color.Trim();
            if (color.Length > ColorMaxLength || color[0] != '#')
                return Error.Validation("expensecategory.color_invalid",
                    "Category color must be a hex value such as #0f766e.");
        }
        else
        {
            color = null;
        }

        return (name, color);
    }

    public static Result<ExpenseCategory> Create(
        Guid tenantId, string name, Guid? parentId, string? color, int displayOrder)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("expensecategory.tenant_required", "Tenant identifier is required.");

        var fields = ValidateFields(name, color);
        if (fields.IsFailure)
            return fields.Error;

        var (n, c) = fields.Value;

        return new ExpenseCategory(Guid.NewGuid(), tenantId, n)
        {
            ParentId = parentId,
            Color = c,
            DisplayOrder = displayOrder,
        };
    }

    public Result Update(string name, string? color, int displayOrder)
    {
        var fields = ValidateFields(name, color);
        if (fields.IsFailure)
            return Result.Failure(fields.Error);

        var (n, c) = fields.Value;

        Name = n;
        Color = c;
        DisplayOrder = displayOrder;

        return Result.Success();
    }

    /// <summary>
    /// Re-parents this category, rejecting any change that would create a cycle (FR-024). The whole
    /// tenant's categories are passed in and walked in memory — a store has tens of categories, and
    /// the alternative is a recursive CTE that LINQ cannot express (research Decision 5).
    /// </summary>
    public Result ChangeParent(Guid? newParentId, IReadOnlyCollection<ExpenseCategory> allCategories)
    {
        if (newParentId == Id)
            return Result.Failure(Error.Validation("expensecategory.cycle",
                "A category cannot be its own parent."));

        if (newParentId is not null)
        {
            var parent = allCategories.FirstOrDefault(c => c.Id == newParentId.Value);
            if (parent is null)
                return Result.Failure(Error.NotFound("expensecategory.parent_not_found",
                    "Parent category was not found."));

            // Walk up from the proposed parent: reaching this category means the move would close a loop.
            var byId = allCategories.ToDictionary(c => c.Id);
            var cursor = parent;
            var guard = 0;
            while (cursor is not null)
            {
                if (cursor.Id == Id)
                    return Result.Failure(Error.Validation("expensecategory.cycle",
                        "The chosen parent is a descendant of this category."));

                if (++guard > allCategories.Count)
                    return Result.Failure(Error.Validation("expensecategory.cycle",
                        "The category hierarchy contains a cycle."));

                cursor = cursor.ParentId is null ? null : byId.GetValueOrDefault(cursor.ParentId.Value);
            }
        }

        ParentId = newParentId;
        return Result.Success();
    }

    /// <summary>
    /// The categories every store starts with (FR-029), so the module is usable before anything is
    /// configured. Seeded per tenant rather than as EF HasData, which produces fixed keys shared
    /// across tenants and cannot work for a tenant-scoped table (research Decision 9). Colours match
    /// the reference screens so the management P&amp;L legend is meaningful on first use.
    /// </summary>
    public static IReadOnlyList<ExpenseCategory> SeedDefaults(Guid tenantId) =>
    [
        Create(tenantId, "Occupation", null, "#7c3aed", 1).Value,
        Create(tenantId, "Personnel", null, "#0f766e", 2).Value,
        Create(tenantId, "Utilities", null, "#ea580c", 3).Value,
        Create(tenantId, "Administrative", null, "#2563eb", 4).Value,
        Create(tenantId, "Marketing", null, "#ec4899", 5).Value,
    ];

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(Error.Conflict("expensecategory.already_active",
                "Category is already active."));

        IsActive = true;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("expensecategory.already_inactive",
                "Category is already inactive."));

        IsActive = false;
        return Result.Success();
    }
}
