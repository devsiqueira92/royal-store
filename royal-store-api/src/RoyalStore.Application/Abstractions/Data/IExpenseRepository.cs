using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Abstractions.Data;

public interface IExpenseRepository
{
    Task AddAsync(Expense expense, CancellationToken ct = default);

    /// <summary>
    /// The expense alone — enough to read, not enough to settle. Use
    /// <see cref="GetByIdWithPaymentsAsync"/> for anything that touches the balance.
    /// </summary>
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// The aggregate with its payments and attachments loaded via Include, so SettledAmount and
    /// RemainingBalance are correct. Required by every payment operation: the balance rules are
    /// evaluated in memory on the loaded children (Constitution VI — load the aggregate and shape
    /// it in memory rather than relying on a projection that may not translate).
    /// </summary>
    Task<Expense?> GetByIdWithPaymentsAsync(Guid id, CancellationToken ct = default);

    Task AddPaymentAsync(ExpensePayment payment, CancellationToken ct = default);

    Task UpdateAsync(Expense expense, CancellationToken ct = default);

    /// <summary>
    /// The month's expenses with their payments, as flat rows. Status and category filtering happen
    /// in the database; the overdue filter and every total are applied in memory by the handler,
    /// because overdue is derived (FR-020) and a GroupBy projection is not provably translatable
    /// (Constitution VI, research Decision 8). Cancelled expenses are excluded here, since no figure
    /// in the module counts them (FR-008).
    /// </summary>
    Task<IReadOnlyList<Expense>> GetByCompetenceMonthAsync(
        ExpenseListFilter filter, CancellationToken ct = default);

    /// <summary>Expenses whose competence month falls in [from, to), for the category report (FR-044).</summary>
    Task<IReadOnlyList<Expense>> GetByCompetenceRangeAsync(
        DateOnly fromInclusive, DateOnly toExclusive, CancellationToken ct = default);

    /// <summary>
    /// Every expense that is not fully paid or cancelled, with its payments — the raw material for
    /// the overdue balance and the committed bucket of the cash-flow projection (FR-045).
    /// </summary>
    Task<IReadOnlyList<Expense>> GetUnsettledAsync(CancellationToken ct = default);

    /// <summary>True when a bill already exists for this template and cycle (FR-032 fast path).</summary>
    Task<bool> ExistsForCycleAsync(Guid recurrenceId, string cycleKey, CancellationToken ct = default);

    /// <summary>
    /// Stops tracking an expense whose insert was rejected. Without this the failed row stays in
    /// the Added state and every later save in the same unit of work retries it — so one cycle
    /// losing the idempotency race would take the whole generation run down with it.
    /// </summary>
    void Detach(Expense expense);
}
