using FluentValidation;
using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.ManageFinancialAccounts;

public sealed class CreateFinancialAccountValidator : AbstractValidator<CreateFinancialAccountCommand>
{
    public CreateFinancialAccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Financial account name is required.")
            .MaximumLength(FinancialAccount.NameMaxLength);

        RuleFor(x => x.Type).IsInEnum();
    }
}

public sealed class UpdateFinancialAccountValidator : AbstractValidator<UpdateFinancialAccountCommand>
{
    public UpdateFinancialAccountValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Financial account name is required.")
            .MaximumLength(FinancialAccount.NameMaxLength);

        RuleFor(x => x.Type).IsInEnum();
    }
}

public sealed class CreateFinancialAccountCommandHandler
    : IRequestHandler<CreateFinancialAccountCommand, Result<FinancialAccountResponse>>
{
    private readonly IFinancialAccountRepository _accountRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFinancialAccountCommandHandler(
        IFinancialAccountRepository accountRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FinancialAccountResponse>> Handle(
        CreateFinancialAccountCommand command, CancellationToken cancellationToken)
    {
        if (await _accountRepository.ExistsByNameAsync(command.Name.Trim(), null, cancellationToken))
            return Error.Conflict("financialaccount.duplicate_name",
                "A financial account with this name already exists.");

        var result = FinancialAccount.Create(_tenantContext.TenantId, command.Name, command.Type);
        if (result.IsFailure)
            return result.Error;

        await _accountRepository.AddAsync(result.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FinancialAccountResponse.From(result.Value);
    }
}

public sealed class UpdateFinancialAccountCommandHandler
    : IRequestHandler<UpdateFinancialAccountCommand, Result<FinancialAccountResponse>>
{
    private readonly IFinancialAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFinancialAccountCommandHandler(
        IFinancialAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FinancialAccountResponse>> Handle(
        UpdateFinancialAccountCommand command, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(command.Id, cancellationToken);
        if (account is null)
            return Error.NotFound("financialaccount.not_found", "Financial account was not found.");

        if (await _accountRepository.ExistsByNameAsync(command.Name.Trim(), command.Id, cancellationToken))
            return Error.Conflict("financialaccount.duplicate_name",
                "A financial account with this name already exists.");

        var update = account.Update(command.Name, command.Type);
        if (update.IsFailure)
            return update.Error;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FinancialAccountResponse.From(account);
    }
}

public sealed class SetFinancialAccountActiveCommandHandler
    : IRequestHandler<SetFinancialAccountActiveCommand, Result<FinancialAccountResponse>>
{
    private readonly IFinancialAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetFinancialAccountActiveCommandHandler(
        IFinancialAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FinancialAccountResponse>> Handle(
        SetFinancialAccountActiveCommand command, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(command.Id, cancellationToken);
        if (account is null)
            return Error.NotFound("financialaccount.not_found", "Financial account was not found.");

        var transition = command.IsActive ? account.Activate() : account.Deactivate();
        if (transition.IsFailure)
            return transition.Error;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FinancialAccountResponse.From(account);
    }
}

public sealed class DeleteFinancialAccountCommandHandler
    : IRequestHandler<DeleteFinancialAccountCommand, Result>
{
    private readonly IFinancialAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFinancialAccountCommandHandler(
        IFinancialAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteFinancialAccountCommand command, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(command.Id, cancellationToken);
        if (account is null)
            return Result.Failure(Error.NotFound("financialaccount.not_found",
                "Financial account was not found."));

        // An account money has actually left must keep existing, or its payment history loses the
        // answer to "where did this go?" (FR-027).
        if (await _accountRepository.IsReferencedAsync(command.Id, cancellationToken))
            return Result.Failure(Error.Conflict("financialaccount.in_use",
                "This financial account is in use by an expense, a payment or a template. Deactivate it instead."));

        await _accountRepository.RemoveAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
