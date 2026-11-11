using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetSaleDetails;

public record GetSaleDetailsQuery(Guid SaleId) : IRequest<Result<SaleDetailsResponse>>;
