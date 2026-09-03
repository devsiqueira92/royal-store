using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Services.Commands.CreateService;

public record CreateServiceCommand(
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
    List<CreateServiceVariantRequest>? Variants,
    string? PisCofinsCst = null,
    string? IvaCst = null,
    bool? IsSelectiveTax = null) : IRequest<Result<ProductResponse>>;

public record CreateServiceVariantRequest(
    string OptionName,
    decimal? Price,
    decimal? CostPrice);
