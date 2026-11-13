namespace RoyalStore.Domain.Expenses;

/// <summary>How often a recurring expense template generates its next bill.</summary>
public enum RecurrenceFrequency
{
    Weekly = 0,
    Monthly = 1,
    Quarterly = 2,
    Semiannually = 3,
    Yearly = 4,
}
