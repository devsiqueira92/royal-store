using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Fiscal.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Fiscal.Queries.GetIbptTaxById;

public sealed class GetIbptTaxByIdQueryHandler
    : IRequestHandler<GetIbptTaxByIdQuery, Result<IbptTaxResponse>>
{
    private readonly IIbptTaxRepository _repository;

    public GetIbptTaxByIdQueryHandler(IIbptTaxRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IbptTaxResponse>> Handle(
        GetIbptTaxByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<IbptTaxResponse>.Failure(
                Error.NotFound("ibpt_tax.not_found", "IBPT tax entry not found."));

        return Result<IbptTaxResponse>.Success(IbptTaxResponse.From(entity));
    }
}
