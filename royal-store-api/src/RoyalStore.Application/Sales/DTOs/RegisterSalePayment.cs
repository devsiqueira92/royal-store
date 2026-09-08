namespace RoyalStore.Application.Sales.DTOs;

/// <summary>One settlement against the sale. <c>Method</c> and <c>Type</c> map to the domain
/// <c>PaymentMethod</c>; the remaining fields are acquirer detail and apply to cards only.</summary>
public record RegisterSalePayment(
    string Method,
    string? Type,
    string? Brand,
    int? Installments,
    decimal Amount,
    string? AuthCode,
    string? Nsu,
    /// <summary>The selected payment-methods-catalog entry (037), when the terminal knows one.
    /// Optional and additive — omitting it preserves today's exact behavior (037 research.md
    /// Decision 8; contracts/register-sale-api-changes.md).</summary>
    Guid? PaymentId = null);
