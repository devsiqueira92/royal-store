namespace RoyalStore.Domain.Sales;

/// <summary>
/// Card detail for a card settlement. Every field is optional so that cash, Pix and cashback
/// settlements can be recorded without them.
/// </summary>
public sealed record PaymentCardDetails(
    string? Flag,
    string? Type,
    int? Installments)
{
    public static readonly PaymentCardDetails Empty = new(null, null, null);
}
