namespace RoyalStore.Domain.Sales;

/// <summary>Sales aggregates for one <see cref="CashShift"/>, summed from the sales linked to it
/// (039-cash-shift-summary). <c>TotalSales</c> is the amount received — after discounts, without any
/// cashback redeemed. Payment-method totals are classified by the catalog payment type code each sale
/// payment line settled with (01 cash, 03 credit card, 04 debit card); other methods count only
/// toward <c>TotalSales</c>.</summary>
public sealed record ShiftSalesTotals(
    decimal TotalSales,
    decimal CashSales,
    decimal CreditCardSales,
    decimal DebitCardSales)
{
    public static ShiftSalesTotals Empty { get; } = new(0m, 0m, 0m, 0m);

    public decimal CardSales => CreditCardSales + DebitCardSales;
}
