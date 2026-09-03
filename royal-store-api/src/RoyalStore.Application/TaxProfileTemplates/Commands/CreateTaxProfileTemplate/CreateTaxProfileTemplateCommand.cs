using MediatR;
using RoyalStore.Application.TaxProfileTemplates.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.TaxProfileTemplates.Commands.CreateTaxProfileTemplate;

public record CreateTaxProfileTemplateCommand(
    string Name,
    string? Description,
    ItemType TargetItemType,
    string? DefaultCfop,
    string? IcmsCst,
    string? PisCofinsCst,
    string? IvaCst,
    bool IsSelectiveTax,
    bool IsDefault) : IRequest<Result<TaxProfileTemplateResponse>>;
