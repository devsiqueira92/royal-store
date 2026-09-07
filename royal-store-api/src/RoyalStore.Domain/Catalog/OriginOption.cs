using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

/// <summary>
/// A shared, platform-wide fiscal origin classification usable by any store to tag a product
/// or service. Global reference data: shared across all tenants, so it carries no TenantId
/// (same precedent as <see cref="Fiscal.IbptTax"/>). Addressed by <see cref="Code"/>, never by
/// its internal Id, which is never exposed outside this entity.
/// </summary>
public sealed class OriginOption : Entity, IAuditableEntity
{
    public string Code { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private OriginOption() : base(Guid.NewGuid()) { }

    private OriginOption(Guid id, string code, string name, string description)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
    }

    public static Result<OriginOption> Create(string code, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("origin_option.name_required", "Name is required.");

        name = name.Trim();

        if (name.Length > 100)
            return Error.Validation("origin_option.name_too_long", "Name must not exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(description))
            return Error.Validation("origin_option.description_required", "Description is required.");

        description = description.Trim();

        if (description.Length > 500)
            return Error.Validation("origin_option.description_too_long", "Description must not exceed 500 characters.");

        return new OriginOption(Guid.NewGuid(), code, name, description);
    }

    public Result Update(string code, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("origin_option.name_required", "Name is required."));

        name = name.Trim();

        if (name.Length > 100)
            return Result.Failure(Error.Validation("origin_option.name_too_long", "Name must not exceed 100 characters."));

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(Error.Validation("origin_option.description_required", "Description is required."));

        description = description.Trim();

        if (description.Length > 500)
            return Result.Failure(Error.Validation("origin_option.description_too_long", "Description must not exceed 500 characters."));

        Code = code;
        Name = name;
        Description = description;
        return Result.Success();
    }
}
