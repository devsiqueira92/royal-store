using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Commands.OpenCashShift;

public record OpenCashShiftCommand(decimal StartingCash) : IRequest<Result<OpenCashShiftResponse>>;
