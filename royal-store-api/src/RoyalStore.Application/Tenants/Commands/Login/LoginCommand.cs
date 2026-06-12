using MediatR;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Tenants.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<TokenResponse>>;
