using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Queries.GetSalesList;

/// <summary>
/// Flattened repository projection for the sales list: the seller's display name is already
/// resolved (Profile.FullName, falling back to the Identity login name) so the handler needs no
/// further lookups — mirrors <c>CashShiftWithMovementTotals</c>'s role for cash shifts.
/// </summary>
public sealed record SaleWithNamesProjection(
    Guid Id,
    string? SellerName,
    decimal Subtotal,
    decimal DiscountAmount,
    string? CustomerName,
    DateTime SoldAt,
    FulfillmentMode FulfillmentMode);
