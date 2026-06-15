using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Commands.DeactivateSeller;

public record DeactivateSellerCommand(Guid Id) : IRequest<Result>;
