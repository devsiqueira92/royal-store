using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

/// <summary>
/// A store-owned unit of measure usable to tag how a store's products/services are sold or
/// counted. Tenant-scoped reference data, structurally modeled on <see cref="Category"/>.
/// </summary>
public sealed class UnitMeasureOption : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private UnitMeasureOption() : base(Guid.NewGuid()) { }

    private UnitMeasureOption(Guid id, Guid tenantId, string code, string name, string description)
        : base(id)
    {
        TenantId = tenantId;
        Code = code;
        Name = name;
        Description = description;
    }

    public static Result<UnitMeasureOption> Create(Guid tenantId, string code, string name, string description)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("unit_measure_option.tenant_required", "Tenant identifier is required.");
        
        if (string.IsNullOrWhiteSpace(code))
            return Error.Validation("unit_measure_option.code_required", "Code is required.");

        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("unit_measure_option.name_required", "Name is required.");

        name = name.Trim();

        if (name.Length > 100)
            return Error.Validation("unit_measure_option.name_too_long", "Name must not exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(description))
            return Error.Validation("unit_measure_option.description_required", "Description is required.");

        description = description.Trim();

        if (description.Length > 500)
            return Error.Validation("unit_measure_option.description_too_long", "Description must not exceed 500 characters.");

        return new UnitMeasureOption(Guid.NewGuid(), tenantId, code, name, description);
    }

    public Result Update(string code, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("unit_measure_option.name_required", "Name is required."));

        name = name.Trim();

        if (name.Length > 100)
            return Result.Failure(Error.Validation("unit_measure_option.name_too_long", "Name must not exceed 100 characters."));

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(Error.Validation("unit_measure_option.description_required", "Description is required."));

        description = description.Trim();

        if (description.Length > 500)
            return Result.Failure(Error.Validation("unit_measure_option.description_too_long", "Description must not exceed 500 characters."));

        Code = code;
        Name = name;
        Description = description;
        return Result.Success();
    }
}
