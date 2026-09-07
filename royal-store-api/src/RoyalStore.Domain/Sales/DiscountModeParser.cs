using RoyalStore.Domain.Common;

namespace RoyalStore.Domain.Sales;

/// <summary>
/// Maps the point-of-sale terminal's free-text discount mode onto <see cref="DiscountMode"/>.
/// An absent or empty value means the discount was entered as a currency amount.
/// </summary>
public static class DiscountModeParser
{
    public static Result<DiscountMode> Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DiscountMode.Amount;

        return value.Trim().ToLowerInvariant() switch
        {
            "r$" or "brl" or "amount" or "valor" => DiscountMode.Amount,
            "%" or "percent" or "percentage" or "percentual" => DiscountMode.Percentage,
            _ => Error.Validation("sale.discount_mode_invalid",
                $"Discount mode '{value}' is not recognised. Expected 'R$' or '%'.")
        };
    }
}
