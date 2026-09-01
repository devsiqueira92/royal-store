using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Fiscal;

/// <summary>
/// A single row of the Brazilian IBPT tax-burden reference table. Global reference data:
/// shared across all tenants, so it carries no <c>TenantId</c>. Populated by loading a CSV
/// directly into the database; the factory below exists for constitution compliance and any
/// future write path, not for the bulk load.
/// </summary>
public sealed class IbptTax : Entity
{
    public string Code { get; private set; } = string.Empty;
    public string? Exception { get; private set; }
    public int Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal NationalRate { get; private set; }
    public decimal FederalRate { get; private set; }
    public decimal ImportedFederalRate { get; private set; }
    public decimal StateRate { get; private set; }
    public decimal MunicipalRate { get; private set; }
    public DateOnly EffectiveStart { get; private set; }
    public DateOnly EffectiveEnd { get; private set; }
    public string? Key { get; private set; }
    public string? Version { get; private set; }
    public string? Source { get; private set; }
    public bool IsActive { get; private set; }

    private IbptTax() : base(Guid.NewGuid()) { }

    private IbptTax(
        Guid id, string code, string? exception, int type, string description,
        decimal nationalRate, decimal federalRate, decimal importedFederalRate,
        decimal stateRate, decimal municipalRate,
        DateOnly effectiveStart, DateOnly effectiveEnd,
        string? key, string? version, string? source)
        : base(id)
    {
        Code = code;
        Exception = exception;
        Type = type;
        Description = description;
        NationalRate = nationalRate;
        FederalRate = federalRate;
        ImportedFederalRate = importedFederalRate;
        StateRate = stateRate;
        MunicipalRate = municipalRate;
        EffectiveStart = effectiveStart;
        EffectiveEnd = effectiveEnd;
        Key = key;
        Version = version;
        Source = source;
        IsActive = true;
    }

    public static Result<IbptTax> Create(
        string code, string? exception, int type, string description,
        decimal nationalRate, decimal federalRate, decimal importedFederalRate,
        decimal stateRate, decimal municipalRate,
        DateOnly effectiveStart, DateOnly effectiveEnd,
        string? key, string? version, string? source)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Error.Validation("ibpt_tax.code_required", "Code is required.");

        code = code.Trim();

        if (code.Length > 20)
            return Error.Validation("ibpt_tax.code_too_long", "Code must not exceed 20 characters.");

        if (string.IsNullOrWhiteSpace(description))
            return Error.Validation("ibpt_tax.description_required", "Description is required.");

        description = description.Trim();

        if (description.Length > 500)
            return Error.Validation("ibpt_tax.description_too_long", "Description must not exceed 500 characters.");

        if (!IsRateInRange(nationalRate) || !IsRateInRange(federalRate) || !IsRateInRange(importedFederalRate) ||
            !IsRateInRange(stateRate) || !IsRateInRange(municipalRate))
            return Error.Validation("ibpt_tax.rate_out_of_range", "Tax-burden rates must be between 0 and 100.");

        if (effectiveEnd < effectiveStart)
            return Error.Validation("ibpt_tax.invalid_period", "Effective end must not be before effective start.");

        exception = string.IsNullOrWhiteSpace(exception) ? null : exception.Trim();

        return new IbptTax(
            Guid.NewGuid(), code, exception, type, description,
            nationalRate, federalRate, importedFederalRate, stateRate, municipalRate,
            effectiveStart, effectiveEnd, key, version, source);
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(Error.Conflict("ibpt_tax.already_active", "Entry is already active."));

        IsActive = true;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("ibpt_tax.already_inactive", "Entry is already inactive."));

        IsActive = false;
        return Result.Success();
    }

    private static bool IsRateInRange(decimal rate) => rate is >= 0m and <= 100m;
}
