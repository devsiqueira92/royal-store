using MediatR;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Queries.GetLabelConfigurations;

/// <summary>
/// The configurations a new label can start from (066): the system configurations, then the store's
/// active labels, each group by name. <c>code</c> = label id (open it with <c>GET api/labels/{id}</c>),
/// <c>description</c> = name.
/// </summary>
public sealed record GetLabelConfigurationsQuery : IRequest<Result<IReadOnlyList<OptionResponse>>>;
