using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sales;

public sealed class Sale : Entity, IAuditableEntity
{
    private readonly List<SaleItem> _items = [];
    private readonly List<SalePayment> _payments = [];

    public Guid TenantId { get; private set; }
    public Guid CashShiftId { get; private set; }
    public string? UserId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public DateTime SoldAt { get; private set; }
    public DateTime SyncedAt { get; private set; }
    public InvoiceStatus FiscalStatus { get; private set; }
    public string? ExternalInvoiceId { get; private set; }
    public string? ReceiptPrintUrl { get; private set; }
    public string? FiscalErrorMessage { get; private set; }

    public IReadOnlyList<SaleItem> Items => _items.AsReadOnly();
    public IReadOnlyList<SalePayment> Payments => _payments.AsReadOnly();

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Sale() : base(Guid.NewGuid()) { }

    private Sale(Guid id, Guid tenantId, Guid cashShiftId, string? userId,
        decimal totalAmount, decimal discountAmount, DateTime soldAt, DateTime syncedAt)
        : base(id)
    {
        TenantId = tenantId;
        CashShiftId = cashShiftId;
        UserId = userId;
        TotalAmount = totalAmount;
        DiscountAmount = discountAmount;
        SoldAt = soldAt;
        SyncedAt = syncedAt;
        FiscalStatus = InvoiceStatus.Pending;
    }

    public static Result<Sale> Create(Guid id, Guid tenantId, Guid cashShiftId, string? userId,
        decimal totalAmount, decimal discountAmount, DateTime soldAt, DateTime syncedAt)
    {
        if (id == Guid.Empty)
            return Error.Validation("sale.id_required", "Sale ID is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("sale.tenant_required", "Tenant identifier is required.");

        if (cashShiftId == Guid.Empty)
            return Error.Validation("sale.cash_shift_required", "Cash shift identifier is required.");

        if (totalAmount < 0)
            return Error.Validation("sale.total_amount_negative", "Total amount cannot be negative.");

        if (discountAmount < 0)
            return Error.Validation("sale.discount_negative", "Discount amount cannot be negative.");

        if (discountAmount > totalAmount)
            return Error.Validation("sale.discount_exceeds_total", "Discount amount cannot exceed total amount.");

        return new Sale(id, tenantId, cashShiftId, userId, totalAmount, discountAmount, soldAt, syncedAt);
    }

    public Result UpdateFiscalStatus(InvoiceStatus newStatus, string? externalInvoiceId,
        string? receiptPrintUrl, string? errorMessage)
    {
        FiscalStatus = newStatus;
        ExternalInvoiceId = newStatus == InvoiceStatus.Authorized ? externalInvoiceId : null;
        ReceiptPrintUrl = newStatus == InvoiceStatus.Authorized ? receiptPrintUrl : null;
        FiscalErrorMessage = newStatus == InvoiceStatus.Rejected ? errorMessage : null;
        return Result.Success();
    }
}
