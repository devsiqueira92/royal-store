using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

/// <summary>
/// A named pricing condition a store works with ("Padrão", "À Vista / Pix", "A Prazo"). A product
/// holds at most one <see cref="ProductPrice"/> per list. Every store has exactly one default list,
/// and the default list is always active (061 FR-003, FR-004).
/// </summary>
public sealed class PriceList : Entity, IAuditableEntity
{
    public const int NameMaxLength = 100;
    public const string DefaultListName = "Padrão";

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>Trimmed, upper-invariant <see cref="Name"/>. Backs the per-store unique index so
    /// "A Prazo" and " a prazo " collide (FR-002). Written only by <see cref="Create"/> and
    /// <see cref="Rename"/>.</summary>
    public string NormalizedName { get; private set; } = string.Empty;

    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private PriceList() : base(Guid.NewGuid()) { }

    private PriceList(Guid id, Guid tenantId, string name, bool isDefault)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        NormalizedName = Normalize(name);
        IsDefault = isDefault;
        IsActive = true;
    }

    /// <summary>The form names are compared in. Callers use it to look a name up before saving.</summary>
    public static string Normalize(string name) => name.Trim().ToUpperInvariant();

    private static Result<string> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("pricelist.name_required", "Price list name is required.");

        name = name.Trim();
        if (name.Length > NameMaxLength)
            return Error.Validation("pricelist.name_too_long",
                $"Price list name must not exceed {NameMaxLength} characters.");

        return name;
    }

    public static Result<PriceList> Create(Guid tenantId, string name)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("pricelist.tenant_required", "Tenant identifier is required.");

        var validName = ValidateName(name);
        if (validName.IsFailure)
            return validName.Error;

        return new PriceList(Guid.NewGuid(), tenantId, validName.Value, isDefault: false);
    }

    /// <summary>The list every store starts with (FR-007): default and active.</summary>
    public static IReadOnlyList<PriceList> SeedDefaults(Guid tenantId) =>
        [new PriceList(Guid.NewGuid(), tenantId, DefaultListName, isDefault: true)];

    public Result Rename(string name)
    {
        var validName = ValidateName(name);
        if (validName.IsFailure)
            return Result.Failure(validName.Error);

        Name = validName.Value;
        NormalizedName = Normalize(validName.Value);
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(Error.Conflict("pricelist.already_active", "Price list is already active."));

        IsActive = true;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (IsDefault)
            return Result.Failure(Error.Conflict("pricelist.default_cannot_be_deactivated",
                "The default price list cannot be deactivated. Choose another default first."));

        if (!IsActive)
            return Result.Failure(Error.Conflict("pricelist.already_inactive", "Price list is already inactive."));

        IsActive = false;
        return Result.Success();
    }

    /// <summary>
    /// Makes this list the store's default. Sales charge the default-list price, so the switch is
    /// refused while any product lacks a price here (FR-005) — there is no fallback to another list.
    /// </summary>
    /// <param name="productsWithoutPrice">How many of the store's products have no price on this list.</param>
    public Result MarkAsDefault(int productsWithoutPrice)
    {
        var allowed = CanBecomeDefault(productsWithoutPrice);
        if (allowed.IsFailure)
            return allowed;

        IsDefault = true;
        return Result.Success();
    }

    /// <summary>
    /// The rule behind <see cref="MarkAsDefault"/>, without the change. A swap has to unmark the
    /// current default and persist that before marking this one, so it asks first.
    /// </summary>
    public Result CanBecomeDefault(int productsWithoutPrice)
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("pricelist.inactive_cannot_be_default",
                "An inactive price list cannot be the default."));

        if (productsWithoutPrice > 0)
            return Result.Failure(Error.Conflict("pricelist.default_missing_prices",
                $"{productsWithoutPrice} products have no price on this list."));

        return Result.Success();
    }

    /// <summary>Only ever one half of a default swap — the other list is marked in the same transaction.</summary>
    public void UnmarkDefault() => IsDefault = false;

    public Result EnsureDeletable(bool hasProductPrices)
    {
        if (IsDefault)
            return Result.Failure(Error.Conflict("pricelist.default_cannot_be_deleted",
                "The default price list cannot be deleted. Choose another default first."));

        if (hasProductPrices)
            return Result.Failure(Error.Conflict("pricelist.in_use",
                "This price list has product prices. Deactivate it instead."));

        return Result.Success();
    }
}
