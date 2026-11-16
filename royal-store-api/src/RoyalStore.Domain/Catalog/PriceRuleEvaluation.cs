namespace RoyalStore.Domain.Catalog;

/// <summary>How a product's stored price on a list relates to that list's percentage rule (062).</summary>
public enum PriceRuleStatus
{
    /// <summary>The list has no rule, or the product has no price on the base list to calculate from.</summary>
    None = 0,

    /// <summary>The stored price is exactly what the rule suggests today.</summary>
    Matches = 1,

    /// <summary>The stored price is not what the rule suggests today — typed over, or the percentage
    /// or the base-list price changed since it was saved.</summary>
    Differs = 2
}

/// <summary>
/// The outcome of holding a stored price against its list's rule. Derived when a product is read and
/// never stored: a product price is always a concrete amount, not a formula.
/// </summary>
public sealed record PriceRuleEvaluation(decimal? SuggestedPrice, PriceRuleStatus Status)
{
    public static readonly PriceRuleEvaluation None = new(null, PriceRuleStatus.None);
}
