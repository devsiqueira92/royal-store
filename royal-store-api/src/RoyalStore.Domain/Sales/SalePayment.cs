using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sales;

public sealed class SalePayment : Entity
{
    public Guid TenantId { get; private set; }
    public Guid SaleId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public decimal Amount { get; private set; }

    private SalePayment() : base(Guid.NewGuid()) { }

    private SalePayment(Guid id, Guid tenantId, Guid saleId, PaymentMethod method, decimal amount)
        : base(id)
    {
        TenantId = tenantId;
        SaleId = saleId;
        Method = method;
        Amount = amount;
    }

    public static Result<SalePayment> Create(Guid id, Guid tenantId, Guid saleId, PaymentMethod method, decimal amount)
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

        return new SalePayment(id, tenantId, saleId, method, amount);
    }
}
