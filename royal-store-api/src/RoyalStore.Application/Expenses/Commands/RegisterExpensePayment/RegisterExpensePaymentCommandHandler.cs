using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.RegisterExpensePayment;

/// <summary>
/// Registers one settlement against an expense.
/// </summary>
/// <remarks>
/// Note what this handler does NOT do: it never touches the cash shift or creates a cash movement,
/// whatever the financial account's type, and it never requires a shift to be open (FR-019a,
/// clarified 2026-11-12). Operating-expense payments stay entirely inside this module.
/// </remarks>
public sealed class RegisterExpensePaymentCommandHandler
    : IRequestHandler<RegisterExpensePaymentCommand, Result<ExpenseResponse>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly IFinancialAccountRepository _financialAccountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterExpensePaymentCommandHandler> _logger;

    public RegisterExpensePaymentCommandHandler(
        IExpenseRepository expenseRepository,
        IExpenseCategoryRepository categoryRepository,
        IFinancialAccountRepository financialAccountRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegisterExpensePaymentCommandHandler> logger)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _financialAccountRepository = financialAccountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ExpenseResponse>> Handle(
        RegisterExpensePaymentCommand command, CancellationToken cancellationToken)
    {
        // Must load the payments: the overpayment rule is evaluated against the settled total.
        var expense = await _expenseRepository.GetByIdWithPaymentsAsync(command.ExpenseId, cancellationToken);
        if (expense is null)
            return Error.NotFound("expense.not_found", "Expense was not found.");

        var account = await _financialAccountRepository.GetByIdAsync(command.FinancialAccountId, cancellationToken);
        if (account is null)
            return Error.NotFound("financialaccount.not_found", "Financial account was not found.");

        if (!account.IsActive)
            return Error.Validation("financialaccount.inactive",
                "This financial account is inactive and cannot be used for new payments.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var paymentResult = expense.RegisterPayment(
            command.FinancialAccountId, command.Amount, command.PaidAt,
            command.MethodNote, command.Notes, today);

        if (paymentResult.IsFailure)
        {
            _logger.LogInformation(
                "Expense payment refused for {ExpenseId}: {ErrorCode}. Attempted {Amount}, remaining {RemainingBalance}.",
                expense.Id, paymentResult.Error.Code, command.Amount, expense.RemainingBalance);

            return paymentResult.Error;
        }

        // No UpdateAsync(expense): the aggregate is already tracked, so its Version bump is detected
        // on save. Calling Update would mark the new payment Modified instead of Added — the same
        // trap RecordCashMovementCommandHandler documents.
        await _expenseRepository.AddPaymentAsync(paymentResult.Value, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another payment committed against this expense after it was loaded, so the balance
            // the check above used is stale. Nothing is recorded — this is what makes two
            // simultaneous payments unable to settle more than the amount (FR-019, SC-004).
            _logger.LogWarning(
                "Concurrent payment rejected for expense {ExpenseId}. Attempted {Amount}.",
                expense.Id, command.Amount);

            return Error.Conflict("expense.concurrent_update",
                "The expense was changed by another operation. Refresh and try again.");
        }

        _logger.LogInformation(
            "Expense payment registered. Expense {ExpenseId}, amount {Amount}, settled {SettledAmount} of {TotalAmount}, status {NewStatus}.",
            expense.Id, command.Amount, expense.SettledAmount, expense.Amount,
            ExpenseResponse.ToStatusName(expense.Status));

        var category = await _categoryRepository.GetByIdAsync(expense.ExpenseCategoryId, cancellationToken);

        return ExpenseResponse.From(
            expense,
            category is null
                ? new ExpenseCategorySummary(expense.ExpenseCategoryId, string.Empty, null)
                : ExpenseCategorySummary.From(category),
            today);
    }
}
