using MediatR;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Queries.GetSellers;

public record GetSellersQuery(bool? IsActive) : IRequest<Result<IReadOnlyList<SellerResponse>>>;
