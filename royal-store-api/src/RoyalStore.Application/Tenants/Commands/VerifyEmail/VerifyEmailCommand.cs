using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Tenants.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string UserId, string Token) : IRequest<Result>;
