using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Payments;

/// <summary>
/// One way a tenant can receive payment (e.g., Cash, Pix, Credit Card). Tenant-scoped and never
/// deleted — only activated/deactivated — so a sale that references one always resolves to a valid
/// record, even years later (FR-004, FR-009). Display name and fiscal code are fixed at seed time;
/// only <see cref="Discount"/> and <see cref="Active"/> are ever editable (data-model.md §Payment).
/// </summary>
public sealed class Payment : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public string PaymentType { get; private set; } = string.Empty;
    public int Discount { get; private set; }
    public bool Active { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Payment() : base(Guid.NewGuid()) { }

    private Payment(Guid id, Guid tenantId, string paymentMethod, string paymentType, int discount, bool active)
        : base(id)
    {
        TenantId = tenantId;
        PaymentMethod = paymentMethod;
        PaymentType = paymentType;
        Discount = discount;
        Active = active;
    }

    public static Result<Payment> Create(Guid tenantId, string paymentMethod, string paymentType, int discount = 0)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("payment.tenant_required", "Tenant identifier is required.");

        if (string.IsNullOrWhiteSpace(paymentMethod))
            return Error.Validation("payment.method_required", "Payment method name is required.");

        paymentMethod = paymentMethod.Trim();
        if (paymentMethod.Length > 50)
            return Error.Validation("payment.method_too_long", "Payment method name must not exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(paymentType))
            return Error.Validation("payment.type_required", "Fiscal payment type code is required.");

        paymentType = paymentType.Trim();
        if (paymentType.Length != 2)
            return Error.Validation("payment.type_invalid_length", "Fiscal payment type code must be exactly 2 characters.");

        var discountCheck = ValidateDiscount(discount);
        if (discountCheck.IsFailure)
            return discountCheck.Error;

        return new Payment(Guid.NewGuid(), tenantId, paymentMethod, paymentType, discount, active: true);
    }

    /// <summary>Idempotent — activating an already-active method is a no-op success (data-model.md).</summary>
    public Result Activate()
    {
        Active = true;
        return Result.Success();
    }

    /// <summary>
    /// Idempotent — deactivating an already-inactive method is a no-op success. Whether this would
    /// leave the tenant with zero active methods (FR-012) is a tenant-wide rule this entity cannot
    /// see on its own; it is enforced by the command handler before this method is ever called
    /// (research.md Decision 11).
    /// </summary>
    public Result Deactivate()
    {
        Active = false;
        return Result.Success();
    }

    public Result UpdateDiscount(int discount)
    {
        var discountCheck = ValidateDiscount(discount);
        if (discountCheck.IsFailure)
            return discountCheck;

        Discount = discount;
        return Result.Success();
    }

    private static Result ValidateDiscount(int discount)
    {
        if (discount < 0 || discount > 100)
            return Result.Failure(Error.Validation("payment.discount_out_of_range",
                "Discount must be between 0 and 100."));

        return Result.Success();
    }

    /// <summary>
    /// The default catalog every tenant starts with (FR-003). Only the fiscal-code-backed methods
    /// are seeded — "split" and "bank slip" have no corresponding code and are deliberately omitted
    /// (spec.md Clarifications; research.md Decision 2). Called both when a new tenant registers and,
    /// once, to backfill every tenant that already existed when this feature shipped.
    /// </summary>
    public static IReadOnlyList<Payment> SeedDefaults(Guid tenantId) =>
    [
        Create(tenantId, "Cash", Code(FiscalPaymentType.Cash)).Value,
        Create(tenantId, "Credit Card", Code(FiscalPaymentType.CreditCard)).Value,
        Create(tenantId, "Debit Card", Code(FiscalPaymentType.DebitCard)).Value,
        Create(tenantId, "Store Card", Code(FiscalPaymentType.StoreCard)).Value,
        Create(tenantId, "Gift Card", Code(FiscalPaymentType.GiftCard)).Value,
        Create(tenantId, "Pix (Dynamic)", Code(FiscalPaymentType.PixDynamic)).Value,
        Create(tenantId, "Cashback / Loyalty Credit", Code(FiscalPaymentType.CashbackOrLoyalty)).Value,
        Create(tenantId, "Pix (Static)", Code(FiscalPaymentType.PixStatic)).Value,
        Create(tenantId, "Store Credit", Code(FiscalPaymentType.StoreCredit)).Value,
    ];

    private static string Code(FiscalPaymentType type) => ((int)type).ToString("D2");
}
