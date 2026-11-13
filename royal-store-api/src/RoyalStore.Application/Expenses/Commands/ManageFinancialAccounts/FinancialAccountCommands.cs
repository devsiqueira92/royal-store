using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.ManageFinancialAccounts;

/// <summary>
/// Adds a financial account (FR-026). No opening balance is accepted, because an account stores no
/// balance at all (FR-026a).
/// </summary>
public record CreateFinancialAccountCommand(string Name, FinancialAccountType Type)
    : IRequest<Result<FinancialAccountResponse>>;

/// <summary>Renames a financial account or changes its type.</summary>
public record UpdateFinancialAccountCommand(Guid Id, string Name, FinancialAccountType Type)
    : IRequest<Result<FinancialAccountResponse>>;

/// <summary>Activates or deactivates a financial account (FR-028).</summary>
public record SetFinancialAccountActiveCommand(Guid Id, bool IsActive)
    : IRequest<Result<FinancialAccountResponse>>;

/// <summary>Deletes an unused account; one money has left is refused (FR-027).</summary>
public record DeleteFinancialAccountCommand(Guid Id) : IRequest<Result>;
