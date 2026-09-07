namespace RoyalStore.Domain.Sales;

/// <summary>
/// How the cashier expressed the sale discount. The stored discount is always a currency amount;
/// this only records how it was entered, so a discounted sale can be explained after the fact.
/// </summary>
public enum DiscountMode
{
    Amount = 0,
    Percentage = 1
}
