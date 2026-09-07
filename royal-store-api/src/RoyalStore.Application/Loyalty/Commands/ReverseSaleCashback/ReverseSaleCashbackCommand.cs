using MediatR;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Commands.ReverseSaleCashback;

public record ReverseSaleCashbackCommand(Guid SaleId) : IRequest<Result<ReverseSaleCashbackResponse>>;
