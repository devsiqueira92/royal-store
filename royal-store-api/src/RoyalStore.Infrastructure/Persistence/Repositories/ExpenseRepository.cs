using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class ExpenseRepository : IExpenseRepository
{
    private readonly RoyalStoreContext _context;

    public ExpenseRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Expense expense, CancellationToken ct = default) =>
        await _context.Expenses.AddAsync(expense, ct);

    public async Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Expense?> GetByIdWithPaymentsAsync(Guid id, CancellationToken ct = default) =>
        await _context.Expenses
            .Include(e => e.Payments)
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddPaymentAsync(ExpensePayment payment, CancellationToken ct = default) =>
        await _context.ExpensePayments.AddAsync(payment, ct);

    public Task UpdateAsync(Expense expense, CancellationToken ct = default)
    {
        _context.Expenses.Update(expense);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Expense>> GetByCompetenceMonthAsync(
        ExpenseListFilter filter, CancellationToken ct = default)
    {
        var query = _context.Expenses
            .Include(e => e.Payments)
            .Where(e => e.CompetenceMonth == filter.CompetenceMonth
                     && e.Status != ExpenseStatus.Cancelled);

        if (filter.Status.HasValue)
            query = query.Where(e => e.Status == filter.Status.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(e => e.ExpenseCategoryId == filter.CategoryId.Value);

        // The overdue filter is deliberately NOT applied here: it depends on the settled total,
        // which only exists once the payments are loaded. The handler applies it in memory.
        return await query
            .OrderBy(e => e.DueDate)
            .ThenBy(e => e.Description)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Expense>> GetByCompetenceRangeAsync(
        DateOnly fromInclusive, DateOnly toExclusive, CancellationToken ct = default) =>
        await _context.Expenses
            .Where(e => e.CompetenceMonth >= fromInclusive
                     && e.CompetenceMonth < toExclusive
                     && e.Status != ExpenseStatus.Cancelled)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Expense>> GetUnsettledAsync(CancellationToken ct = default) =>
        await _context.Expenses
            .Include(e => e.Payments)
            .Where(e => e.Status != ExpenseStatus.Paid && e.Status != ExpenseStatus.Cancelled)
            .ToListAsync(ct);

    public async Task<bool> ExistsForCycleAsync(
        Guid recurrenceId, string cycleKey, CancellationToken ct = default) =>
        await _context.Expenses.AnyAsync(
            e => e.ExpenseRecurrenceId == recurrenceId && e.RecurrenceCycleKey == cycleKey, ct);

    public void Detach(Expense expense) =>
        _context.Entry(expense).State = EntityState.Detached;
}
