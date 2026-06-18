using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Fiscal.Commands.ProcessFiscalInvoice;

public record ProcessFiscalInvoiceCommand(Guid SaleId) : IRequest<Result>;
