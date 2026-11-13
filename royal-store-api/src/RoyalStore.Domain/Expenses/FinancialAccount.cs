using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Expenses;

/// <summary>
/// Where money actually leaves from — the cash drawer, a bank account or a digital wallet (FR-026).
/// </summary>
/// <remarks>
/// A classification only: there is deliberately NO opening balance, running balance or any stored
/// total (FR-026a, clarified 2026-11-12). Outflow attributed to an account is computed on demand
/// from <see cref="ExpensePayment"/> rows. A <see cref="FinancialAccountType.Cash"/> account has no
/// relationship whatsoever to the cash shift / cash movement ledger (FR-019a) — this module holds no
/// reference to either type.
/// </remarks>
public sealed class FinancialAccount : Entity, IAuditableEntity
{
    public const int NameMaxLength = 100;

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public FinancialAccountType Type { get; private set; }
    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private FinancialAccount() : base(Guid.NewGuid()) { }

    private FinancialAccount(Guid id, Guid tenantId, string name, FinancialAccountType type)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Type = type;
        IsActive = true;
    }

    private static Result<string> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("financialaccount.name_required", "Financial account name is required.");

        name = name.Trim();
        if (name.Length > NameMaxLength)
            return Error.Validation("financialaccount.name_too_long",
                $"Financial account name must not exceed {NameMaxLength} characters.");

        return name;
    }

    public static Result<FinancialAccount> Create(Guid tenantId, string name, FinancialAccountType type)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("financialaccount.tenant_required", "Tenant identifier is required.");

        if (!Enum.IsDefined(type))
            return Error.Validation("financialaccount.type_invalid", "Financial account type is invalid.");

        var validated = ValidateName(name);
        if (validated.IsFailure)
            return validated.Error;

        return new FinancialAccount(Guid.NewGuid(), tenantId, validated.Value, type);
    }

    public Result Update(string name, FinancialAccountType type)
    {
        if (!Enum.IsDefined(type))
            return Result.Failure(Error.Validation("financialaccount.type_invalid",
                "Financial account type is invalid."));

        var validated = ValidateName(name);
        if (validated.IsFailure)
            return Result.Failure(validated.Error);

        Name = validated.Value;
        Type = type;

        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(Error.Conflict("financialaccount.already_active",
                "Financial account is already active."));

        IsActive = true;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("financialaccount.already_inactive",
                "Financial account is already inactive."));

        IsActive = false;
        return Result.Success();
    }
}
