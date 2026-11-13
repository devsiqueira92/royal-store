namespace RoyalStore.Domain.Expenses;

/// <summary>Where money leaves from when an expense is settled.</summary>
public enum FinancialAccountType
{
    Cash = 0,
    BankAccount = 1,
    DigitalWallet = 2,
}
