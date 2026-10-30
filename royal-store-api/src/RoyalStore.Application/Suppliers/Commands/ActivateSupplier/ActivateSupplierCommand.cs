using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Suppliers.Commands.ActivateSupplier;

public record ActivateSupplierCommand(Guid Id) : IRequest<Result>;
