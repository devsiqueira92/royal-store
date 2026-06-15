using MediatR;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Queries.GetSellerById;

public record GetSellerByIdQuery(Guid Id) : IRequest<Result<SellerResponse>>;
