using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sales;

public sealed class Sale : Entity, IAuditableEntity
{
    /// <summary>Rounding tolerance, in currency units, when reconciling the server's own totals.</summary>
    public const decimal ReconciliationTolerance = 0.01m;

    private readonly List<SaleItem> _items = [];
    private readonly List<SalePayment> _payments = [];

    public Guid TenantId { get; private set; }

    /// <summary>Reserved for a future cash-register-session feature. Accepted and stored as-is;
    /// never required or validated against anything today (038-register-sale-checkout FR-007).</summary>
    public Guid? CashShiftId { get; private set; }

    public string? UserId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public decimal Subtotal { get; private set; }

    /// <summary>The seller-entered manual sale-level discount — the one genuine client input among
    /// the checkout totals (research.md Decision 8).</summary>
    public decimal DiscountAmount { get; private set; }

    public decimal LoyaltyDiscountApplied { get; private set; }
    public decimal PaymentDiscountApplied { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal CashbackApplied { get; private set; }
    public decimal CashbackEarned { get; private set; }

    /// <summary>The customer's usable cashback balance immediately after this sale. Meaningless
    /// (zero) when the sale has no linked customer.</summary>
    public decimal CashbackRemaining { get; private set; }

    public FulfillmentMode FulfillmentMode { get; private set; }
    public string? BranchId { get; private set; }

    /// <summary>Delivery/pickup address, set once at creation. Never null —
    /// <see cref="SaleAddress.Empty"/> when none was given.</summary>
    public SaleAddress Address { get; private set; } = SaleAddress.Empty;

    public SaleCustomerSnapshot? CustomerSnapshot { get; private set; }
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

    private Sale(Guid id, Guid tenantId, Guid? cashShiftId, string? userId, Guid? customerId,
        decimal subtotal, decimal discountAmount, decimal loyaltyDiscountApplied, decimal paymentDiscountApplied,
        decimal taxAmount, decimal totalAmount, decimal cashbackApplied, decimal cashbackRemaining,
        FulfillmentMode fulfillmentMode, string? branchId, SaleAddress address, SaleCustomerSnapshot? customerSnapshot,
        DateTime soldAt, DateTime syncedAt)
        : base(id)
    {
        TenantId = tenantId;
        CashShiftId = cashShiftId;
        UserId = userId;
        CustomerId = customerId;
        Subtotal = subtotal;
        DiscountAmount = discountAmount;
        LoyaltyDiscountApplied = loyaltyDiscountApplied;
        PaymentDiscountApplied = paymentDiscountApplied;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
        CashbackApplied = cashbackApplied;
        CashbackEarned = 0;
        CashbackRemaining = cashbackRemaining;
        FulfillmentMode = fulfillmentMode;
        BranchId = branchId;
        Address = address;
        CustomerSnapshot = customerSnapshot;
        SoldAt = soldAt;
        SyncedAt = syncedAt;
        FiscalStatus = InvoiceStatus.Pending;
    }

    public static Result<Sale> Create(Guid id, Guid tenantId, Guid? cashShiftId, string? userId, Guid? customerId,
        decimal subtotal, decimal discountAmount, decimal loyaltyDiscountApplied, decimal paymentDiscountApplied,
        decimal taxAmount, decimal totalAmount, decimal cashbackApplied, decimal cashbackRemaining,
        FulfillmentMode fulfillmentMode, string? branchId, SaleAddress address, SaleCustomerSnapshot? customerSnapshot,
        DateTime soldAt, DateTime syncedAt)
    {
        if (id == Guid.Empty)
            return Error.Validation("sale.id_required", "Sale ID is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("sale.tenant_required", "Tenant identifier is required.");

        if (cashShiftId == Guid.Empty)
            return Error.Validation("sale.cash_shift_invalid", "Cash register session identifier cannot be empty.");

        if (subtotal < 0)
            return Error.Validation("sale.subtotal_negative", "Subtotal cannot be negative.");

        if (discountAmount < 0)
            return Error.Validation("sale.discount_negative", "Discount amount cannot be negative.");

        if (discountAmount > subtotal)
            return Error.Validation("sale.discount_exceeds_subtotal", "Discount amount cannot exceed the subtotal.");

        if (loyaltyDiscountApplied < 0)
            return Error.Validation("sale.loyalty_discount_negative", "Loyalty discount cannot be negative.");

        if (paymentDiscountApplied < 0)
            return Error.Validation("sale.payment_discount_negative", "Payment method discount cannot be negative.");

        if (taxAmount < 0)
            return Error.Validation("sale.tax_negative", "Tax amount cannot be negative.");

        if (totalAmount < 0)
            return Error.Validation("sale.total_amount_negative", "Total amount cannot be negative.");

        if (cashbackApplied < 0)
            return Error.Validation("sale.cashback_applied_negative", "Cashback applied cannot be negative.");

        if (cashbackRemaining < 0)
            return Error.Validation("sale.cashback_remaining_negative", "Cashback remaining cannot be negative.");

        if (!Enum.IsDefined(fulfillmentMode))
            return Error.Validation("sale.fulfillment_mode_invalid", "Fulfillment mode is invalid.");

        if (fulfillmentMode == FulfillmentMode.Pickup && string.IsNullOrWhiteSpace(branchId))
            return Error.Validation("sale.branch_required_for_pickup", "A branch is required for in-store pickup.");

        if (address is null)
            return Error.Validation("sale.address_required", "Sale address must be provided; use SaleAddress.Empty for none.");

        // Server-authoritative invariant (research.md Decision 8): every discount and the tax amount
        // are computed independently server-side; this only confirms they add up to the total that
        // gets persisted and sent downstream to fiscal document generation.
        if (Math.Abs(subtotal - discountAmount - loyaltyDiscountApplied - paymentDiscountApplied
                     - cashbackApplied + taxAmount - totalAmount) > ReconciliationTolerance)
            return Error.Validation("sale.totals_do_not_reconcile",
                "Subtotal less every discount less cashback plus tax must equal the total.");

        return new Sale(id, tenantId, cashShiftId, userId, customerId, subtotal, discountAmount,
            loyaltyDiscountApplied, paymentDiscountApplied, taxAmount, totalAmount, cashbackApplied,
            cashbackRemaining, fulfillmentMode, branchId?.Trim(), address, customerSnapshot, soldAt, syncedAt);
    }

    /// <summary>Records what this sale earned, once, after the ledger computes it from the earning
    /// base — set here rather than in the constructor because it's only known after items are built
    /// and the ledger has run (data-model.md §Sale).</summary>
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
