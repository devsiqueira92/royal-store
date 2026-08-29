using MediatR;
using RoyalStore.Application.Permissions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Permissions.Queries.GetPermissionCatalog;

public record GetPermissionCatalogQuery : IRequest<Result<PermissionCatalogResponse>>;
