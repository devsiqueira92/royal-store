using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Commands.SyncSales;

public record SyncSalesCommand(IReadOnlyList<SaleSyncRequest> Sales) : IRequest<Result<SyncSalesResponse>>;
