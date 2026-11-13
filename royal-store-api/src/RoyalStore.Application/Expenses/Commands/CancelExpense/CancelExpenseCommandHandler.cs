using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.CancelExpense;

public sealed class CancelExpenseCommandHandler
    : IRequestHandler<CancelExpenseCommand, Result<ExpenseResponse>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelExpenseCommandHandler> _logger;

    public CancelExpenseCommandHandler(
        IExpenseRepository expenseRepository,
        IExpenseCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ILogger<CancelExpenseCommandHandler> logger)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ExpenseResponse>> Handle(
        CancelExpenseCommand command, CancellationToken cancellationToken)
    {
        // Payments are needed: Cancel refuses while an unreversed payment exists (FR-007).
        var expense = await _expenseRepository.GetByIdWithPaymentsAsync(command.Id, cancellationToken);
        if (expense is null)
            return Error.NotFound("expense.not_found", "Expense was not found.");

        var cancellation = expense.Cancel();
        if (cancellation.IsFailure)
        {
            _logger.LogInformation(
                "Expense cancellation refused for {ExpenseId}: {ErrorCode}.",
                expense.Id, cancellation.Error.Code);

            return cancellation.Error;
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.Conflict("expense.concurrent_update",
                "The expense was changed by another operation. Refresh and try again.");
        }

        _logger.LogInformation(
            "Expense {ExpenseId} cancelled. Amount {Amount}, payee {PayeeName}.",
            expense.Id, expense.Amount, expense.PayeeName);

        var category = await _categoryRepository.GetByIdAsync(expense.ExpenseCategoryId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return ExpenseResponse.From(
            expense,
            category is null
                ? new ExpenseCategorySummary(expense.ExpenseCategoryId, string.Empty, null)
                : ExpenseCategorySummary.From(category),
            today);
    }
}
