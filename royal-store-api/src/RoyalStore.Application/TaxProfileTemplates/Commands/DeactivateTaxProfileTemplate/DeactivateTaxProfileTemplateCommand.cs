using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.TaxProfileTemplates.Commands.DeactivateTaxProfileTemplate;

public record DeactivateTaxProfileTemplateCommand(Guid Id) : IRequest<Result>;
