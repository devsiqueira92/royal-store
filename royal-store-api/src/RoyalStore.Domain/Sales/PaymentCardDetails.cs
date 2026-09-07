namespace RoyalStore.Domain.Sales;

/// <summary>
/// Acquirer detail for a card settlement. Every field is optional so that cash and instant-transfer
/// payments can be recorded without them.
/// </summary>
public sealed record PaymentCardDetails(
    string? Type,
    string? Brand,
    int? Installments,
    string? AuthCode,
    string? Nsu)
{
    public static readonly PaymentCardDetails Empty = new(null, null, null, null, null);
}
