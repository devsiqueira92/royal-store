using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Loyalty;

/// <summary>
/// A configurable classification — VIP, Gold, Silver, Standard, or others an administrator adds.
/// Governs the cashback accrual rate and how long cashback earned under it stays valid. Tenant-scoped.
/// </summary>
public sealed class LoyaltyTier : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal CashbackPercentage { get; private set; }
    public int CashbackValidityDays { get; private set; }
    public bool Active { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private LoyaltyTier() : base(Guid.NewGuid()) { }

    private LoyaltyTier(Guid id, Guid tenantId, string name, decimal cashbackPercentage, int cashbackValidityDays, bool active)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        CashbackPercentage = cashbackPercentage;
        CashbackValidityDays = cashbackValidityDays;
        Active = active;
    }

    public static Result<LoyaltyTier> Create(Guid tenantId, string name, decimal cashbackPercentage, int cashbackValidityDays)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("loyaltytier.tenant_required", "Tenant identifier is required.");

        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("loyaltytier.name_required", "Tier name is required.");

        name = name.Trim();
        if (name.Length > 50)
            return Error.Validation("loyaltytier.name_too_long", "Tier name must not exceed 50 characters.");

        var percentageCheck = ValidatePercentage(cashbackPercentage);
        if (percentageCheck.IsFailure)
            return percentageCheck.Error;

        var validityCheck = ValidateValidityDays(cashbackValidityDays);
        if (validityCheck.IsFailure)
            return validityCheck.Error;

        return new LoyaltyTier(Guid.NewGuid(), tenantId, name, cashbackPercentage, cashbackValidityDays, active: true);
    }

    /// <summary>Idempotent — activating an already-active tier is a no-op success.</summary>
    public Result Activate()
    {
        Active = true;
        return Result.Success();
    }

    /// <summary>Idempotent — deactivating an already-inactive tier is a no-op success. A deactivated
    /// tier stops accruing future cashback; cashback already earned under it is untouched.</summary>
    public Result Deactivate()
    {
        Active = false;
        return Result.Success();
    }

    public Result Update(decimal cashbackPercentage, int cashbackValidityDays)
    {
        var percentageCheck = ValidatePercentage(cashbackPercentage);
        if (percentageCheck.IsFailure)
            return percentageCheck;

        var validityCheck = ValidateValidityDays(cashbackValidityDays);
        if (validityCheck.IsFailure)
            return validityCheck;

        CashbackPercentage = cashbackPercentage;
        CashbackValidityDays = cashbackValidityDays;
        return Result.Success();
    }

    private static Result ValidatePercentage(decimal cashbackPercentage)
    {
        if (cashbackPercentage < 0 || cashbackPercentage > 100)
            return Result.Failure(Error.Validation("loyaltytier.percentage_out_of_range",
                "Cashback percentage must be between 0 and 100."));

        return Result.Success();
    }

    private static Result ValidateValidityDays(int cashbackValidityDays)
    {
        if (cashbackValidityDays <= 0)
            return Result.Failure(Error.Validation("loyaltytier.validity_invalid",
                "Cashback validity period must be greater than zero days."));

        return Result.Success();
    }

    /// <summary>
    /// The four tiers every tenant starts with (FR-010). Percentages and the 90-day validity are
    /// reasonable starting values — an administrator can change any of them immediately (FR-009a,
    /// FR-011). Called both when a new tenant registers and, once, to backfill every tenant that
    /// already existed when this feature shipped.
    /// </summary>
    public static IReadOnlyList<LoyaltyTier> SeedDefaults(Guid tenantId) =>
    [
        Create(tenantId, "VIP", 10.0m, 90).Value,
        Create(tenantId, "Gold", 7.0m, 90).Value,
        Create(tenantId, "Silver", 4.0m, 90).Value,
        Create(tenantId, "Standard", 2.0m, 90).Value,
    ];
}
