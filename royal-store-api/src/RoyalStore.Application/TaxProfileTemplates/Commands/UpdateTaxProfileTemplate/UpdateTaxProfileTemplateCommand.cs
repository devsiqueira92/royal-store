using MediatR;
using RoyalStore.Application.TaxProfileTemplates.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.TaxProfileTemplates.Commands.UpdateTaxProfileTemplate;

public record UpdateTaxProfileTemplateCommand(
    Guid Id,
    string Name,
    string? Description,
    string? DefaultCfop,
    string? IcmsCst,
    string? PisCofinsCst,
    string? IvaCst,
    bool IsSelectiveTax,
    bool IsDefault) : IRequest<Result<TaxProfileTemplateResponse>>;
