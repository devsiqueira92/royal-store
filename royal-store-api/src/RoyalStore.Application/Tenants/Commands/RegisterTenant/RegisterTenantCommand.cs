using MediatR;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Tenants;

namespace RoyalStore.Application.Tenants.Commands.RegisterTenant;

public sealed record RegisterTenantCommand(
    string Email,
    string Document,
    DocumentType DocumentType,
    string StoreName,
    string Password) : IRequest<Result<RegistrationResponse>>;
