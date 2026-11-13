using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Common;

/// <summary>
/// Verifies that the category, cost center and financial account an expense or template points at
/// exist and are usable. Shared by the create, update and recurrence handlers so the rule is stated
/// once.
/// </summary>
/// <remarks>
/// Existence is checked through the repositories, which read through the tenant query filter — a
/// reference to another store's category therefore comes back as not found, never as a leak
/// (FR-009). Deactivated entries are refused on new work while existing references stay intact
/// (FR-028).
/// </remarks>
public sealed class ExpenseClassificationGuard
{
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IFinancialAccountRepository _financialAccountRepository;

    public ExpenseClassificationGuard(
        IExpenseCategoryRepository categoryRepository,
        ICostCenterRepository costCenterRepository,
        IFinancialAccountRepository financialAccountRepository)
    {
        _categoryRepository = categoryRepository;
        _costCenterRepository = costCenterRepository;
        _financialAccountRepository = financialAccountRepository;
    }

    /// <summary>Returns the resolved category on success; the optional references are only validated.</summary>
    public async Task<Result<ExpenseCategory>> ValidateAsync(
        Guid categoryId,
        Guid? costCenterId,
        Guid? financialAccountId,
        bool requireActive,
        CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, ct);
        if (category is null)
            return Error.NotFound("expensecategory.not_found", "Expense category was not found.");

        if (requireActive && !category.IsActive)
            return Error.Validation("expensecategory.inactive",
                "This expense category is inactive and cannot be used on new entries.");

        if (costCenterId is not null)
        {
            var costCenter = await _costCenterRepository.GetByIdAsync(costCenterId.Value, ct);
            if (costCenter is null)
                return Error.NotFound("costcenter.not_found", "Cost center was not found.");

            if (requireActive && !costCenter.IsActive)
                return Error.Validation("costcenter.inactive",
                    "This cost center is inactive and cannot be used on new entries.");
        }

        if (financialAccountId is not null)
        {
            var account = await _financialAccountRepository.GetByIdAsync(financialAccountId.Value, ct);
            if (account is null)
                return Error.NotFound("financialaccount.not_found", "Financial account was not found.");

            if (requireActive && !account.IsActive)
                return Error.Validation("financialaccount.inactive",
                    "This financial account is inactive and cannot be used on new entries.");
        }

        return category;
    }
}
