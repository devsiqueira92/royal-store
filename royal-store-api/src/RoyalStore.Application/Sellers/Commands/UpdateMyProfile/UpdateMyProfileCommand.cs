using MediatR;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Commands.UpdateMyProfile;

public record UpdateMyProfileCommand(
    string FullName,
    string? Phone,
    string? Email) : IRequest<Result<CurrentSellerResponse>>;
