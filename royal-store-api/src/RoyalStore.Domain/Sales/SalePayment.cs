using RoyalStore.Domain.Common;
using RoyalStore.Domain.Payments;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sales;

public sealed class SalePayment : Entity
{
    public Guid TenantId { get; private set; }
    public Guid SaleId { get; private set; }
    public FiscalPaymentType FiscalType { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentCardDetails Card { get; private set; } = PaymentCardDetails.Empty;

    /// <summary>The catalog <c>Payment</c> (037-payment-methods-catalog) this line settles against.
    /// Always required — every checkout line now references a resolved, active, tenant-owned catalog
    /// entry; resolving/validating it happens one layer up, in the command handler, against live
    /// tenant data this entity cannot see (data-model.md §SalePayment).</summary>
    public Guid PaymentId { get; private set; }

    private SalePayment() : base(Guid.NewGuid()) { }

    private SalePayment(Guid id, Guid tenantId, Guid saleId, FiscalPaymentType fiscalType,
        decimal amount, PaymentCardDetails card, Guid paymentId)
        : base(id)
    {
        TenantId = tenantId;
        SaleId = saleId;
        FiscalType = fiscalType;
        Amount = amount;
        Card = card;
        PaymentId = paymentId;
    }

    public static Result<SalePayment> Create(Guid id, Guid tenantId, Guid saleId,
        FiscalPaymentType fiscalType, decimal amount, Guid paymentId, PaymentCardDetails? card = null)
    {
        if (id == Guid.Empty)
            return Error.Validation("salepayment.id_required", "Sale payment ID is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("salepayment.tenant_required", "Tenant identifier is required.");

        if (saleId == Guid.Empty)
            return Error.Validation("salepayment.sale_required", "Sale identifier is required.");

        if (!Enum.IsDefined(fiscalType))
            return Error.Validation("sale.fiscal_type_invalid", "Fiscal payment type is invalid.");

        if (amount <= 0)
            return Error.Validation("salepayment.amount_invalid", "Payment amount must be greater than zero.");

        if (paymentId == Guid.Empty)
            return Error.Validation("salepayment.payment_method_required", "Payment method reference is required.");

        card ??= PaymentCardDetails.Empty;

        var cardResult = ValidateCard(card);
        if (cardResult.IsFailure)
            return cardResult.Error;

        return new SalePayment(id, tenantId, saleId, fiscalType, amount, card, paymentId);
    }

    private static Result ValidateCard(PaymentCardDetails card)
    {
        if (card.Installments is not null)
        {
            if (card.Installments < 1)
                return Result.Failure(Error.Validation("salepayment.installments_invalid",
                    "Installment count must be at least one."));

            if (card.Type != "credit")
                return Result.Failure(Error.Validation("sale.installments_not_applicable",
                    "Installments apply to credit-card payments only."));
        }

        if (card.Type is { Length: > 20 })
            return Result.Failure(Error.Validation("salepayment.type_too_long",
                "Payment type must not exceed 20 characters."));

        if (card.Flag is { Length: > 30 })
            return Result.Failure(Error.Validation("salepayment.flag_too_long",
                "Card flag must not exceed 30 characters."));

        return Result.Success();
    }
}
