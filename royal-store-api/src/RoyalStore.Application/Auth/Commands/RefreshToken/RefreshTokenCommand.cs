using MediatR;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand : IRequest<Result<TokenResponse>>;
