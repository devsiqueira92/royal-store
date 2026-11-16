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

    /// <summary>−100% takes the whole price off (a free item); anything lower would be negative.</summary>
    public const decimal MinDiscountPercentage = -100m;
    public const decimal MaxDiscountPercentage = 999.99m;

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>Trimmed, upper-invariant <see cref="Name"/>. Backs the per-store unique index so
    /// "A Prazo" and " a prazo " collide (FR-002). Written only by <see cref="Create"/> and
    /// <see cref="Rename"/>.</summary>
    public string NormalizedName { get; private set; } = string.Empty;

    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// The percentage rule (062): how this list's price relates to the price on
    /// <see cref="BasePriceListId"/>. <b>Negative is a discount, positive is a surcharge</b> — the
    /// name says "discount" but −5 means "5% off" and +10 means "10% more". Null with
    /// <see cref="BasePriceListId"/> null means the list is priced by hand. The two are always both
    /// set or both null.
    /// <para>
    /// The rule only ever produces a <i>suggestion</i> (<see cref="SuggestPrice"/>). A product's
    /// price on this list is whatever was saved for it; changing the rule never touches it.
    /// </para>
    /// </summary>
    public decimal? DiscountPercentage { get; private set; }

    /// <summary>The list this one is calculated from. Rules are one level deep: a base list has no
    /// rule of its own.</summary>
    public Guid? BasePriceListId { get; private set; }

    public bool HasRule => BasePriceListId.HasValue;

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

    /// <param name="listsBasedOnThis">How many lists use this one as the base of their rule.</param>
    public Result Deactivate(int listsBasedOnThis)
    {
        var free = EnsureNotBaseOfOthers(listsBasedOnThis);
        if (free.IsFailure)
            return free;

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

    /// <param name="hasProductPrices">True when at least one product is priced on this list.</param>
    /// <param name="listsBasedOnThis">How many lists use this one as the base of their rule.</param>
    public Result EnsureDeletable(bool hasProductPrices, int listsBasedOnThis)
    {
        var free = EnsureNotBaseOfOthers(listsBasedOnThis);
        if (free.IsFailure)
            return free;

        if (IsDefault)
            return Result.Failure(Error.Conflict("pricelist.default_cannot_be_deleted",
                "The default price list cannot be deleted. Choose another default first."));

        if (hasProductPrices)
            return Result.Failure(Error.Conflict("pricelist.in_use",
                "This price list has product prices. Deactivate it instead."));

        return Result.Success();
    }

    /// <summary>
    /// Gives this list a percentage rule, or changes the one it has (062 FR-001 to FR-005).
    /// No product price is affected.
    /// </summary>
    /// <param name="baseList">The list the price is calculated from.</param>
    /// <param name="percentage">Negative = discount, positive = surcharge; two decimals at most.</param>
    /// <param name="listsBasedOnThis">How many lists use this one as the base of their rule.</param>
    public Result SetRule(PriceList baseList, decimal percentage, int listsBasedOnThis)
    {
        if (percentage < MinDiscountPercentage || percentage > MaxDiscountPercentage)
            return Result.Failure(Error.Validation("pricelist.percentage_out_of_range",
                $"The percentage must be between {MinDiscountPercentage} and {MaxDiscountPercentage}."));

        // Told, not silently rounded: someone who typed −5.555 should know it was not taken as is.
        if (decimal.Round(percentage, 2) != percentage)
            return Result.Failure(Error.Validation("pricelist.percentage_too_precise",
                "The percentage accepts at most two decimal places."));

        // A list of another store is simply not a list this one can see.
        if (baseList.TenantId != TenantId)
            return Result.Failure(Error.Validation("pricelist.base_not_found", "The base price list was not found."));

        if (baseList.Id == Id)
            return Result.Failure(Error.Validation("pricelist.base_is_self",
                "A price list cannot be based on itself."));

        if (!baseList.IsActive)
            return Result.Failure(Error.Conflict("pricelist.base_inactive",
                $"Price list '{baseList.Name}' is inactive and cannot be used as a base."));

        // One level only: a suggestion always comes straight from a hand-priced list, so it never
        // depends on the order in which other suggestions were made.
        if (baseList.HasRule)
            return Result.Failure(Error.Conflict("pricelist.base_has_rule",
                $"Price list '{baseList.Name}' has a rule of its own and cannot be used as a base."));

        var free = EnsureNotBaseOfOthers(listsBasedOnThis);
        if (free.IsFailure)
            return free;

        DiscountPercentage = percentage;
        BasePriceListId = baseList.Id;
        return Result.Success();
    }

    /// <summary>Back to manual pricing. No product price is affected.</summary>
    public void ClearRule()
    {
        DiscountPercentage = null;
        BasePriceListId = null;
    }

    /// <summary>
    /// What the rule yields for a price on the base list: <c>basePrice × (1 + percentage ÷ 100)</c>,
    /// rounded to two decimals, half away from zero (062 FR-011). Null when the list has no rule.
    /// <para>
    /// The rounding mode is stated on purpose: the framework default is banker's rounding, which
    /// turns 0.025 into 0.02 — not what a shopkeeper expects, and not what the product screen,
    /// which pre-fills with this same formula, would show.
    /// </para>
    /// </summary>
    public decimal? SuggestPrice(decimal basePrice)
    {
        if (DiscountPercentage is not { } percentage)
            return null;

        return Math.Round(basePrice * (1 + percentage / 100m), 2, MidpointRounding.AwayFromZero);
    }

    private static Result EnsureNotBaseOfOthers(int listsBasedOnThis) =>
        listsBasedOnThis > 0
            ? Result.Failure(Error.Conflict("pricelist.is_base_of_others",
                listsBasedOnThis == 1
                    ? "1 price list is based on this list. Remove its rule first."
                    : $"{listsBasedOnThis} price lists are based on this list. Remove their rule first."))
            : Result.Success();
}
