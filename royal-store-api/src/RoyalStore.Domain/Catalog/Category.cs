using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

public sealed class Category : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Category() : base(Guid.NewGuid()) { }

    private Category(Guid id, Guid tenantId, string name, string? description)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        IsActive = true;
    }

    public static Result<Category> Create(Guid tenantId, string name, string? description)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("category.tenant_required", "Tenant identifier is required.");

        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("category.name_required", "Category name is required.");

        name = name.Trim();

        if (name.Length > 100)
            return Error.Validation("category.name_too_long", "Category name must not exceed 100 characters.");

        if (description is not null && description.Length > 500)
            return Error.Validation("category.description_too_long", "Category description must not exceed 500 characters.");

        return new Category(Guid.NewGuid(), tenantId, name, description);
    }

    public Result Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("category.name_required", "Category name is required."));

        name = name.Trim();

        if (name.Length > 100)
            return Result.Failure(Error.Validation("category.name_too_long", "Category name must not exceed 100 characters."));

        if (description is not null && description.Length > 500)
            return Result.Failure(Error.Validation("category.description_too_long", "Category description must not exceed 500 characters."));

        Name = name;
        Description = description;
        return Result.Success();
    }

    public Result Disable()
    {
        if (!IsActive)
            return Result.Failure(Error.Validation("category.already_disabled", "Category is already inactive."));

        IsActive = false;
        return Result.Success();
    }

    public ICollection<Product> Products { get; private set; } = [];
}
