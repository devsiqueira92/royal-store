using RoyalStore.Domain.Common;

namespace RoyalStore.Domain.Sales;

/// <summary>
/// Maps the checkout's wire fulfillment mode string onto <see cref="FulfillmentMode"/>.
/// </summary>
public static class FulfillmentModeParser
{
    public static Result<FulfillmentMode> Parse(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "pickup" => FulfillmentMode.Pickup,
            "delivery" => FulfillmentMode.Delivery,
            _ => Error.Validation("sale.fulfillment_mode_invalid",
                $"Fulfillment mode '{value}' is not recognised. Expected 'pickup' or 'delivery'.")
        };
    }
}
