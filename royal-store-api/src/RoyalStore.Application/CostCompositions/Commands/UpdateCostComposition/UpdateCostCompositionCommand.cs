using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.CostCompositions.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.CostCompositions.Commands.UpdateCostComposition;

/// <summary>
/// Changes a list's name and lines (064 FR-010, FR-027). The set sent is the complete list: lines with an id are
/// updated, lines without one are added, stored lines not sent are removed. Works whether or not the
/// list is linked to a product, and never touches the product (FR-019).
/// </summary>
public record UpdateCostCompositionCommand(Guid Id, string Name, IReadOnlyList<ProductCostItemInput> Items)
    : IRequest<Result<CostCompositionResponse>>
{
    // A body without "items" binds as null; an empty list is a valid list.
    public IReadOnlyList<ProductCostItemInput> Items { get; init; } = Items ?? [];
}

public sealed class UpdateCostCompositionValidator : AbstractValidator<UpdateCostCompositionCommand>
{
    public UpdateCostCompositionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cost composition name is required.")
            .MaximumLength(CostCompositionList.NameMaxLength);

        RuleFor(x => x).Custom((command, context) =>
            CostCompositionInputs.ValidateShape(command.Items, context));
    }
}

public sealed class UpdateCostCompositionCommandHandler
    : IRequestHandler<UpdateCostCompositionCommand, Result<CostCompositionResponse>>
{
    private readonly ICostCompositionRepository _costCompositionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateCostCompositionCommandHandler> _logger;

    public UpdateCostCompositionCommandHandler(
        ICostCompositionRepository costCompositionRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<UpdateCostCompositionCommandHandler> logger)
    {
        _costCompositionRepository = costCompositionRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<CostCompositionResponse>> Handle(
        UpdateCostCompositionCommand command, CancellationToken cancellationToken)
    {
        var list = await _costCompositionRepository.GetByIdAsync(command.Id, cancellationToken);
        if (list is null)
            return Error.NotFound("costcomposition.not_found", "Cost composition was not found.");

        var renamed = list.Rename(command.Name);
        if (renamed.IsFailure)
            return renamed.Error;

        var replaced = list.ReplaceItems(command.Items);
        if (replaced.IsFailure)
            return replaced.Error;

        // The lines are final, so the total can be taken now.
        list.RecalculateTotal();

        // Children added to an already-tracked list are registered explicitly: a child with a
        // client-generated key discovered on a tracked graph is mistaken for an existing row.
        foreach (var item in replaced.Value.Added)
            await _costCompositionRepository.AddItemAsync(item, cancellationToken);

        foreach (var item in replaced.Value.Removed)
            _costCompositionRepository.RemoveItem(item);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cost composition {CostCompositionId} of tenant {TenantId} now has {ItemCount} items.",
            list.Id, _tenantContext.TenantId, list.Items.Count);

        return CostCompositionResponse.From(list);
    }
}
