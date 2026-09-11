namespace RoyalStore.Application.Sales.DTOs;

/// <summary>One settlement line against the sale. <c>MethodId</c> always references a
/// payment-methods-catalog (037) entry — resolved and validated server-side, never trusted from the
/// request beyond structural presence. <c>FiscalType</c> is validated against the store's recognized
/// fiscal payment classifications (038-register-sale-checkout FR-005). <c>DiscountRate</c> is
/// informational only — the server derives its own from the resolved catalog entry.</summary>
public record RegisterSalePayment(
    Guid MethodId,
    int FiscalType,
    decimal Amount,
    decimal DiscountRate,
    RegisterSalePaymentCard? Card = null);
