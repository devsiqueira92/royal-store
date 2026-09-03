using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.TaxProfileTemplates.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.TaxProfileTemplates.Queries.GetTaxProfileTemplates;

public sealed class GetTaxProfileTemplatesQueryHandler
    : IRequestHandler<GetTaxProfileTemplatesQuery, Result<IReadOnlyList<TaxProfileTemplateResponse>>>
{
    private readonly ITaxProfileTemplateRepository _repository;

    public GetTaxProfileTemplatesQueryHandler(ITaxProfileTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<TaxProfileTemplateResponse>>> Handle(
        GetTaxProfileTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var templates = await _repository.GetAllAsync(request.TargetItemType, cancellationToken);

        IReadOnlyList<TaxProfileTemplateResponse> response = templates
            .Select(TaxProfileTemplateResponse.From)
            .ToList();

        return Result<IReadOnlyList<TaxProfileTemplateResponse>>.Success(response);
    }
}
