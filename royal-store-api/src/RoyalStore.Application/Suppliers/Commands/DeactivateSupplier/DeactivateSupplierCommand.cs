using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Suppliers.Commands.DeactivateSupplier;

public record DeactivateSupplierCommand(Guid Id) : IRequest<Result>;
