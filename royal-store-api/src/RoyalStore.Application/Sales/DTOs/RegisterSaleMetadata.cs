namespace RoyalStore.Application.Sales.DTOs;

/// <summary>
/// Terminal context for a checkout. <c>SellerId</c> is accepted for payload compatibility but is
/// deliberately ignored — the operator is taken from the authenticated principal, never from the
/// client. <c>SaleId</c> is an optional client-generated idempotency key.
/// </summary>
public record RegisterSaleMetadata(
    Guid? SaleId,
    string? SellerId,
    Guid CashRegisterSessionId,
    DateTime? SoldAt);
