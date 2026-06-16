using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Commands.CloseCashShift;

public record CloseCashShiftCommand(decimal ActualEndingCash) : IRequest<Result<CloseCashShiftResponse>>;
