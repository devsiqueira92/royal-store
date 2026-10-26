using MediatR;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Queries.GetCurrentSeller;

public sealed record GetCurrentSellerQuery : IRequest<Result<CurrentSellerResponse>>;
