namespace RoyalStore.Application.Sales.DTOs;

/// <summary>
/// Checkout context for a sale registration. <c>CashRegisterSessionId</c> is reserved for a future
/// cash-register-session feature — accepted and stored when present, never required or validated
/// (038-register-sale-checkout FR-007). <c>PriceListId</c> is the active list the lines are charged
/// on; null charges on the store's default list.
/// </summary>
public record RegisterSaleCheckout(
    RegisterSaleCustomerRef? Customer,
    bool UseCashback,
    RegisterSaleFulfillment Fulfillment,
    IReadOnlyList<RegisterSalePayment> Payments,
    RegisterSaleTotals Totals,
    Guid? CashRegisterSessionId = null,
    Guid? PriceListId = null);
