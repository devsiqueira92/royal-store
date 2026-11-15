using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.PriceLists.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.CreatePriceList;

public record CreatePriceListCommand(string Name) : IRequest<Result<PriceListResponse>>;

public sealed class CreatePriceListValidator : AbstractValidator<CreatePriceListCommand>
{
    public CreatePriceListValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Price list name is required.")
            .MaximumLength(PriceList.NameMaxLength);
    }
}

public sealed class CreatePriceListCommandHandler
    : IRequestHandler<CreatePriceListCommand, Result<PriceListResponse>>
{
    private static readonly Error DuplicateName = Error.Conflict(
        "pricelist.name_duplicate", "A price list with this name already exists.");

    private readonly IPriceListRepository _priceListRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUniqueConstraintDetector _uniqueConstraintDetector;

    public CreatePriceListCommandHandler(
        IPriceListRepository priceListRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        IUniqueConstraintDetector uniqueConstraintDetector)
    {
        _priceListRepository = priceListRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _uniqueConstraintDetector = uniqueConstraintDetector;
    }

    public async Task<Result<PriceListResponse>> Handle(
        CreatePriceListCommand command, CancellationToken cancellationToken)
    {
        var created = PriceList.Create(_tenantContext.TenantId, command.Name);
        if (created.IsFailure)
            return created.Error;

        var priceList = created.Value;

        if (await _priceListRepository.ExistsByNormalizedNameAsync(
                priceList.NormalizedName, null, cancellationToken))
            return DuplicateName;

        await _priceListRepository.AddAsync(priceList, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (_uniqueConstraintDetector.IsUniqueViolation(ex))
        {
            // Someone created the same name between the check above and this insert. The unique
            // index is the real guarantee; the check only makes the common case friendlier.
            return DuplicateName;
        }

        return PriceListResponse.From(priceList, productCount: 0);
    }
}
