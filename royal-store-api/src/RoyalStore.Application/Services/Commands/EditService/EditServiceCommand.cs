using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Services.Commands.EditService;

public sealed record ServiceVariantInput(
    string OptionName,
    decimal? Price,
    decimal? CostPrice);

public sealed record ServiceVariantUpdateInput(
    Guid VariantId,
    string OptionName,
    decimal? Price,
    decimal? CostPrice);

public record EditServiceCommand(
    Guid Id,
    int Version,
    Guid CategoryId,
    string Name,
    string Sku,
    decimal BasePrice,
    decimal BaseCostPrice,
    string? Description,
    string UnitOfMeasure,
    string Nbs,
    string IssCode,
    Guid TaxProfileTemplateId,
    bool OverrideTaxProfile,
    IReadOnlyList<ServiceVariantInput> VariantsToAdd,
    IReadOnlyList<ServiceVariantUpdateInput> VariantsToUpdate,
    string? PisCofinsCst = null,
    string? IvaCst = null,
    bool? IsSelectiveTax = null)
    : IRequest<Result<ProductResponse>>
{
    public IReadOnlyList<ServiceVariantInput> VariantsToAdd { get; init; } = VariantsToAdd ?? [];
    public IReadOnlyList<ServiceVariantUpdateInput> VariantsToUpdate { get; init; } = VariantsToUpdate ?? [];
}
