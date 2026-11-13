using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Expenses.Common;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.CreateExpense;

public sealed class CreateExpenseCommandHandler
    : IRequestHandler<CreateExpenseCommand, Result<ExpenseResponse>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ExpenseClassificationGuard _classificationGuard;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateExpenseCommandHandler(
        IExpenseRepository expenseRepository,
        ExpenseClassificationGuard classificationGuard,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
    {
        _expenseRepository = expenseRepository;
        _classificationGuard = classificationGuard;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ExpenseResponse>> Handle(
        CreateExpenseCommand command, CancellationToken cancellationToken)
    {
        var classification = await _classificationGuard.ValidateAsync(
            command.ExpenseCategoryId, command.CostCenterId, command.FinancialAccountId,
            requireActive: true, cancellationToken);

        if (classification.IsFailure)
            return classification.Error;

        var expenseResult = Expense.Create(
            _tenantContext.TenantId,
            command.Description,
            command.PayeeName,
            command.SupplierId,
            command.ExpenseCategoryId,
            command.CostCenterId,
            command.FinancialAccountId,
            command.Amount,
            command.CompetenceMonth,
            command.DueDate,
            command.Notes,
            command.IsEstimated);

        if (expenseResult.IsFailure)
            return expenseResult.Error;

        await _expenseRepository.AddAsync(expenseResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return ExpenseResponse.From(
            expenseResult.Value, ExpenseCategorySummary.From(classification.Value), today);
    }
}
