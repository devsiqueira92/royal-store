using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.ReverseExpensePayment;

public sealed class ReverseExpensePaymentCommandHandler
    : IRequestHandler<ReverseExpensePaymentCommand, Result<ExpenseResponse>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReverseExpensePaymentCommandHandler> _logger;

    public ReverseExpensePaymentCommandHandler(
        IExpenseRepository expenseRepository,
        IExpenseCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReverseExpensePaymentCommandHandler> logger)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ExpenseResponse>> Handle(
        ReverseExpensePaymentCommand command, CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdWithPaymentsAsync(command.ExpenseId, cancellationToken);
        if (expense is null)
            return Error.NotFound("expense.not_found", "Expense was not found.");

        var reversal = expense.ReversePayment(command.PaymentId, DateTimeOffset.UtcNow, command.Reason);
        if (reversal.IsFailure)
            return reversal.Error;

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
            "Expense payment {PaymentId} reversed on expense {ExpenseId}. Settled now {SettledAmount} of {TotalAmount}, status {NewStatus}.",
            command.PaymentId, expense.Id, expense.SettledAmount, expense.Amount,
            ExpenseResponse.ToStatusName(expense.Status));

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
