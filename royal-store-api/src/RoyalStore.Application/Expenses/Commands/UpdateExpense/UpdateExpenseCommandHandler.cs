using MediatR;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.Common;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.UpdateExpense;

public sealed class UpdateExpenseCommandHandler
    : IRequestHandler<UpdateExpenseCommand, Result<ExpenseResponse>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ExpenseClassificationGuard _classificationGuard;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateExpenseCommandHandler(
        IExpenseRepository expenseRepository,
        ExpenseClassificationGuard classificationGuard,
        IUnitOfWork unitOfWork)
    {
        _expenseRepository = expenseRepository;
        _classificationGuard = classificationGuard;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ExpenseResponse>> Handle(
        UpdateExpenseCommand command, CancellationToken cancellationToken)
    {
        // Payments are needed: ChangeAmount compares the new amount against the settled total.
        var expense = await _expenseRepository.GetByIdWithPaymentsAsync(command.Id, cancellationToken);
        if (expense is null)
            return Error.NotFound("expense.not_found", "Expense was not found.");

        var classification = await _classificationGuard.ValidateAsync(
            command.ExpenseCategoryId, command.CostCenterId, command.FinancialAccountId,
            requireActive: true, cancellationToken);

        if (classification.IsFailure)
            return classification.Error;

        var details = expense.UpdateDetails(
            command.Description,
            command.PayeeName,
            command.SupplierId,
            command.ExpenseCategoryId,
            command.CostCenterId,
            command.FinancialAccountId,
            command.CompetenceMonth,
            command.DueDate,
            command.Notes);

        if (details.IsFailure)
            return details.Error;

        if (command.Amount != expense.Amount)
        {
            var amountChange = expense.ChangeAmount(command.Amount);
            if (amountChange.IsFailure)
                return amountChange.Error;
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // A payment committed against this expense after it was loaded, so the settled total the
            // amount check used is stale. Nothing is written and the caller retries.
            return Error.Conflict("expense.concurrent_update",
                "The expense was changed by another operation. Refresh and try again.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return ExpenseResponse.From(expense, ExpenseCategorySummary.From(classification.Value), today);
    }
}
