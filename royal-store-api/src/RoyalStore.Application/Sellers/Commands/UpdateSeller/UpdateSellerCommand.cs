using MediatR;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Commands.UpdateSeller;

/// <summary>
/// Replaces a Seller's individual permission grants. Does not change roles and
/// does not touch onboarding fields.
/// </summary>
public record UpdateSellerCommand(
    Guid SellerId,
    string? Role,
    IReadOnlyList<string> Permissions) : IRequest<Result<SellerPermissionsResponse>>;
