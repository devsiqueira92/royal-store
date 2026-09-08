using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sales;

public sealed class SalePayment : Entity
{
    public Guid TenantId { get; private set; }
    public Guid SaleId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentCardDetails Card { get; private set; } = PaymentCardDetails.Empty;

    /// <summary>Which catalog <c>Payment</c> (037-payment-methods-catalog) the terminal selected for
    /// this line, when it sent one. Null for terminals that don't yet send it and for historical
    /// rows that predate this feature — resolving/validating it happens one layer up, in the command
    /// handler, against live tenant data this entity cannot see (data-model.md §SalePayment).</summary>
    public Guid? PaymentId { get; private set; }

    private SalePayment() : base(Guid.NewGuid()) { }

    private SalePayment(Guid id, Guid tenantId, Guid saleId, PaymentMethod method,
        decimal amount, PaymentCardDetails card, Guid? paymentId)
        : base(id)
    {
        TenantId = tenantId;
        SaleId = saleId;
        Method = method;
        Amount = amount;
        Card = card;
        PaymentId = paymentId;
    }

    public static Result<SalePayment> Create(Guid id, Guid tenantId, Guid saleId,
        PaymentMethod method, decimal amount, PaymentCardDetails? card = null, Guid? paymentId = null)
    {
        if (id == Guid.Empty)
            return Error.Validation("salepayment.id_required", "Sale payment ID is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("salepayment.tenant_required", "Tenant identifier is required.");

        if (saleId == Guid.Empty)
            return Error.Validation("salepayment.sale_required", "Sale identifier is required.");

        if (!Enum.IsDefined(method))
            return Error.Validation("salepayment.method_invalid", "Payment method is invalid.");

        if (amount <= 0)
            return Error.Validation("salepayment.amount_invalid", "Payment amount must be greater than zero.");

        card ??= PaymentCardDetails.Empty;

        var cardResult = ValidateCard(card, method);
        if (cardResult.IsFailure)
            return cardResult.Error;

        return new SalePayment(id, tenantId, saleId, method, amount, card, paymentId);
    }

    private static Result ValidateCard(PaymentCardDetails card, PaymentMethod method)
    {
        if (card.Installments is not null)
        {
            if (card.Installments < 1)
                return Result.Failure(Error.Validation("salepayment.installments_invalid",
                    "Installment count must be at least one."));

            if (method != PaymentMethod.CreditCard)
                return Result.Failure(Error.Validation("salepayment.installments_not_applicable",
                    "Installments apply to credit-card payments only."));
        }

        if (card.Type is { Length: > 20 })
            return Result.Failure(Error.Validation("salepayment.type_too_long",
                "Payment type must not exceed 20 characters."));

        if (card.Brand is { Length: > 30 })
            return Result.Failure(Error.Validation("salepayment.brand_too_long",
                "Card brand must not exceed 30 characters."));

        if (card.AuthCode is { Length: > 50 })
            return Result.Failure(Error.Validation("salepayment.auth_code_too_long",
                "Authorization code must not exceed 50 characters."));

        if (card.Nsu is { Length: > 50 })
            return Result.Failure(Error.Validation("salepayment.nsu_too_long",
                "NSU must not exceed 50 characters."));

        return Result.Success();
    }
}
