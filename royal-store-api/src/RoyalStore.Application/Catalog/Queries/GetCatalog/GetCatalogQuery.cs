using MediatR;
using RoyalStore.Application.Catalog.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Catalog.Queries.GetCatalog;

public record GetCatalogQuery(DateTimeOffset? Since) : IRequest<Result<CatalogSyncResponse>>;
