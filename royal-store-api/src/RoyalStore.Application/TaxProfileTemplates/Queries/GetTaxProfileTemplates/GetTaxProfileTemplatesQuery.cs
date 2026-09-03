using MediatR;
using RoyalStore.Application.TaxProfileTemplates.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.TaxProfileTemplates.Queries.GetTaxProfileTemplates;

public record GetTaxProfileTemplatesQuery(ItemType? TargetItemType) : IRequest<Result<IReadOnlyList<TaxProfileTemplateResponse>>>;
