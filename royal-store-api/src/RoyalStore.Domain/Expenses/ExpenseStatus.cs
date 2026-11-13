namespace RoyalStore.Domain.Expenses;

/// <summary>
/// Lifecycle of an operating expense. Values are explicit so reordering the enum can never
/// silently re-map stored rows (persisted via HasConversion&lt;int&gt;).
/// </summary>
/// <remarks>
/// There is deliberately no OVERDUE member: overdue is derived at read time from the due date
/// (FR-020), which is what lets an expense be both <see cref="PartiallyPaid"/> and overdue.
/// </remarks>
public enum ExpenseStatus
{
    Pending = 0,
    PartiallyPaid = 1,
    Paid = 2,
    Cancelled = 3,
}
