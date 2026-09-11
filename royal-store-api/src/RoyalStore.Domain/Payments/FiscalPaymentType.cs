namespace RoyalStore.Domain.Payments;

/// <summary>
/// Fiscal payment-type codes recognized by the external fiscal reporting API. Used both as a
/// seed-time reference when building the tenant's default <see cref="Payment"/> catalog, and as a
/// live, persisted classification validated on every checkout payment line (038-register-sale-checkout
/// FR-005) — the terminal submits it directly, matching these numeric values 1:1. The persisted
/// <see cref="Payment.PaymentType"/> column is a plain string (the zero-padded code, e.g. "01"), sent
/// to the fiscal API verbatim, not this enum (data-model.md §FiscalPaymentType).
/// </summary>
public enum FiscalPaymentType
{
    Cash = 1,               // "01"
    CreditCard = 3,         // "03"
    DebitCard = 4,          // "04"
    StoreCard = 5,          // "05"
    GiftCard = 12,          // "12"
    PixDynamic = 17,        // "17"
    CashbackOrLoyalty = 19, // "19"
    PixStatic = 20,         // "20"
    StoreCredit = 21,       // "21"
    Other = 99              // "99" — reserved; no seeded default uses it (research.md Decision 2)
}
