using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

public sealed class TaxProfileTemplate : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ItemType TargetItemType { get; private set; }
    public string? DefaultCfop { get; private set; }
    public string? IcmsCst { get; private set; }
    public string? PisCofinsCst { get; private set; }
    public string? IvaCst { get; private set; }
    public bool IsSelectiveTax { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; }

    public ICollection<Product> Products { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private TaxProfileTemplate() : base(Guid.NewGuid()) { }

    private TaxProfileTemplate(Guid id, Guid tenantId, string name, string? description, ItemType targetItemType,
        string? defaultCfop, string? icmsCst, string? pisCofinsCst, string? ivaCst,
        bool isSelectiveTax, bool isDefault)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        TargetItemType = targetItemType;
        DefaultCfop = defaultCfop;
        IcmsCst = icmsCst;
        PisCofinsCst = pisCofinsCst;
        IvaCst = ivaCst;
        IsSelectiveTax = isSelectiveTax;
        IsActive = true;
        IsDefault = isDefault;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Result Validate(string name, ItemType targetItemType, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("tax_profile_template.name_required", "Name is required."));

        if (name.Trim().Length > 100)
            return Result.Failure(Error.Validation("tax_profile_template.name_too_long", "Name must not exceed 100 characters."));

        if (description is not null && description.Length > 500)
            return Result.Failure(Error.Validation("tax_profile_template.description_too_long", "Description must not exceed 500 characters."));

        if (targetItemType is not (ItemType.Product or ItemType.Service))
            return Result.Failure(Error.Validation("tax_profile_template.target_item_type_invalid", "Target item type must be Product or Service."));

        return Result.Success();
    }

    public static Result<TaxProfileTemplate> Create(
        Guid tenantId, string name, string? description, ItemType targetItemType,
        string? defaultCfop, string? icmsCst, string? pisCofinsCst, string? ivaCst,
        bool isSelectiveTax, bool isDefault)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("tax_profile_template.tenant_required", "Tenant identifier is required.");

        var validation = Validate(name, targetItemType, description);
        if (!validation.IsSuccess)
            return validation.Error;

        return new TaxProfileTemplate(Guid.NewGuid(), tenantId, name.Trim(), description, targetItemType,
            Normalize(defaultCfop), Normalize(icmsCst), Normalize(pisCofinsCst), Normalize(ivaCst),
            isSelectiveTax, isDefault);
    }

    public Result Update(
        string name, string? description,
        string? defaultCfop, string? icmsCst, string? pisCofinsCst, string? ivaCst,
        bool isSelectiveTax, bool isDefault)
    {
        var validation = Validate(name, TargetItemType, description);
        if (!validation.IsSuccess)
            return validation;

        Name = name.Trim();
        Description = description;
        DefaultCfop = Normalize(defaultCfop);
        IcmsCst = Normalize(icmsCst);
        PisCofinsCst = Normalize(pisCofinsCst);
        IvaCst = Normalize(ivaCst);
        IsSelectiveTax = isSelectiveTax;
        IsDefault = isDefault;

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("tax_profile_template.already_inactive", "Tax profile template is already inactive."));

        IsActive = false;
        return Result.Success();
    }
}
