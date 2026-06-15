using MediatR;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Commands.UpdateSeller;

public record UpdateSellerCommand(
    Guid Id,
    string FullName,
    string Phone,
    string? UserId) : IRequest<Result<SellerResponse>>;
