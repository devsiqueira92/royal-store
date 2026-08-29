using MediatR;
using RoyalStore.Application.Permissions.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.Application.Permissions.Queries.GetPermissionCatalog;

public sealed class GetPermissionCatalogQueryHandler
    : IRequestHandler<GetPermissionCatalogQuery, Result<PermissionCatalogResponse>>
{
    public Task<Result<PermissionCatalogResponse>> Handle(
        GetPermissionCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var groups = PermissionCatalog.All
            .GroupBy(e => e.Resource)
            .OrderBy(g => g.Key)
            .Select(g => new PermissionResourceGroup(
                g.Key.ToString(),
                g.Select(e => new PermissionItem(e.Name, e.Label)).ToArray()))
            .ToArray();

        var response = new PermissionCatalogResponse(groups);
        return Task.FromResult<Result<PermissionCatalogResponse>>(response);
    }
}
