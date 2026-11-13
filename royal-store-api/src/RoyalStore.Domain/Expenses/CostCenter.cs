using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Expenses;

/// <summary>A named part of the business a cost can be attributed to (FR-025).</summary>
public sealed class CostCenter : Entity, IAuditableEntity
{
    public const int NameMaxLength = 100;
    public const int CodeMaxLength = 20;

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private CostCenter() : base(Guid.NewGuid()) { }

    private CostCenter(Guid id, Guid tenantId, string name)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        IsActive = true;
    }

    private static Result<(string Name, string? Code)> ValidateFields(string name, string? code)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("costcenter.name_required", "Cost center name is required.");

        name = name.Trim();
        if (name.Length > NameMaxLength)
            return Error.Validation("costcenter.name_too_long",
                $"Cost center name must not exceed {NameMaxLength} characters.");

        if (!string.IsNullOrWhiteSpace(code))
        {
            code = code.Trim();
            if (code.Length > CodeMaxLength)
                return Error.Validation("costcenter.code_too_long",
                    $"Cost center code must not exceed {CodeMaxLength} characters.");
        }
        else
        {
            code = null;
        }

        return (name, code);
    }

    public static Result<CostCenter> Create(Guid tenantId, string name, string? code)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("costcenter.tenant_required", "Tenant identifier is required.");

        var fields = ValidateFields(name, code);
        if (fields.IsFailure)
            return fields.Error;

        var (n, c) = fields.Value;

        return new CostCenter(Guid.NewGuid(), tenantId, n) { Code = c };
    }

    public Result Update(string name, string? code)
    {
        var fields = ValidateFields(name, code);
        if (fields.IsFailure)
            return Result.Failure(fields.Error);

        var (n, c) = fields.Value;

        Name = n;
        Code = c;

        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(Error.Conflict("costcenter.already_active", "Cost center is already active."));

        IsActive = true;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("costcenter.already_inactive", "Cost center is already inactive."));

        IsActive = false;
        return Result.Success();
    }
}
