using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Services.Commands.EditService;

public sealed record ServiceVariantInput(string OptionName);

public sealed record ServiceVariantUpdateInput(Guid VariantId, string OptionName);

public record EditServiceCommand(
    Guid Id,
    int Version,
    Guid CategoryId,
    string Name,
    string Sku,
    // The service's one price, stored on the store's default price list (pricing rules for
    // services are a later feature).
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
    bool? IsSelectiveTax = null,
    Guid? SupplierId = null)
    : IRequest<Result<ProductResponse>>
{
    public IReadOnlyList<ServiceVariantInput> VariantsToAdd { get; init; } = VariantsToAdd ?? [];
    public IReadOnlyList<ServiceVariantUpdateInput> VariantsToUpdate { get; init; } = VariantsToUpdate ?? [];
}
