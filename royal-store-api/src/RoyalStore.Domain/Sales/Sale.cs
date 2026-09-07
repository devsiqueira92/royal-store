using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sales;

public sealed class Sale : Entity, IAuditableEntity
{
    /// <summary>Rounding tolerance, in currency units, when reconciling the terminal's own totals.</summary>
    public const decimal ReconciliationTolerance = 0.01m;

    private readonly List<SaleItem> _items = [];
    private readonly List<SalePayment> _payments = [];

    public Guid TenantId { get; private set; }
    public Guid CashShiftId { get; private set; }
    public string? UserId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public DiscountMode DiscountMode { get; private set; }
    public decimal AdditionalFees { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal Change { get; private set; }
    public decimal CashbackApplied { get; private set; }
    public decimal CashbackEarned { get; private set; }
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

    private Sale(Guid id, Guid tenantId, Guid cashShiftId, string? userId, Guid? customerId,
        decimal subtotal, decimal discountAmount, DiscountMode discountMode, decimal additionalFees,
        decimal totalAmount, decimal change, decimal cashbackApplied, DateTime soldAt, DateTime syncedAt)
        : base(id)
    {
        TenantId = tenantId;
        CashShiftId = cashShiftId;
        UserId = userId;
        CustomerId = customerId;
        Subtotal = subtotal;
        DiscountAmount = discountAmount;
        DiscountMode = discountMode;
        AdditionalFees = additionalFees;
        TotalAmount = totalAmount;
        Change = change;
        CashbackApplied = cashbackApplied;
        CashbackEarned = 0;
        SoldAt = soldAt;
        SyncedAt = syncedAt;
        FiscalStatus = InvoiceStatus.Pending;
    }

    public static Result<Sale> Create(Guid id, Guid tenantId, Guid cashShiftId, string? userId, Guid? customerId,
        decimal subtotal, decimal discountAmount, DiscountMode discountMode, decimal additionalFees,
        decimal totalAmount, decimal change, decimal cashbackApplied, DateTime soldAt, DateTime syncedAt)
    {
        if (id == Guid.Empty)
            return Error.Validation("sale.id_required", "Sale ID is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("sale.tenant_required", "Tenant identifier is required.");

        if (cashShiftId == Guid.Empty)
            return Error.Validation("sale.cash_shift_required", "Cash shift identifier is required.");

        if (!Enum.IsDefined(discountMode))
            return Error.Validation("sale.discount_mode_invalid", "Discount mode is invalid.");

        if (subtotal < 0)
            return Error.Validation("sale.subtotal_negative", "Subtotal cannot be negative.");

        if (discountAmount < 0)
            return Error.Validation("sale.discount_negative", "Discount amount cannot be negative.");

        if (additionalFees < 0)
            return Error.Validation("sale.additional_fees_negative", "Additional fees cannot be negative.");

        if (totalAmount < 0)
            return Error.Validation("sale.total_amount_negative", "Total amount cannot be negative.");

        if (change < 0)
            return Error.Validation("sale.change_negative", "Change cannot be negative.");

        if (cashbackApplied < 0)
            return Error.Validation("sale.cashback_applied_negative", "Cashback applied cannot be negative.");

        if (discountAmount > subtotal)
            return Error.Validation("sale.discount_exceeds_subtotal", "Discount amount cannot exceed the subtotal.");

        if (Math.Abs(subtotal - discountAmount + additionalFees - totalAmount) > ReconciliationTolerance)
            return Error.Validation("sale.totals_do_not_reconcile",
                "Subtotal less discount plus additional fees must equal the total.");

        return new Sale(id, tenantId, cashShiftId, userId, customerId, subtotal, discountAmount, discountMode,
            additionalFees, totalAmount, change, cashbackApplied, soldAt, syncedAt);
    }

    /// <summary>Records what this sale earned, once, after the ledger computes it from the earning
    /// base (FR-016a) — set here rather than in the constructor because it's only known after items
    /// are built and the ledger has run (data-model.md §Sale).</summary>
    public Result RecordCashbackEarned(decimal amount)
    {
        if (amount < 0)
            return Result.Failure(Error.Validation("sale.cashback_earned_negative", "Cashback earned cannot be negative."));

        CashbackEarned = amount;
        return Result.Success();
    }

    public Result AddItem(SaleItem item)
    {
        if (item.SaleId != Id)
            return Result.Failure(Error.Validation("sale.item_foreign", "The item does not belong to this sale."));

        _items.Add(item);
        return Result.Success();
    }

    public Result AddPayment(SalePayment payment)
    {
        if (payment.SaleId != Id)
            return Result.Failure(Error.Validation("sale.payment_foreign", "The payment does not belong to this sale."));

        _payments.Add(payment);
        return Result.Success();
    }

    public Result UpdateFiscalStatus(InvoiceStatus newStatus, string? externalInvoiceId,
        string? receiptPrintUrl, string? errorMessage)
    {
        FiscalStatus = newStatus;
        ExternalInvoiceId = newStatus == InvoiceStatus.Authorized ? externalInvoiceId : null;
        ReceiptPrintUrl = newStatus == InvoiceStatus.Authorized ? receiptPrintUrl : null;
        FiscalErrorMessage = newStatus is InvoiceStatus.Rejected or InvoiceStatus.Pending ? errorMessage : null;
        return Result.Success();
    }
}
