using MediatR;
using RoyalStore.Application.Labels.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Queries.GetLabels;

/// <summary>
/// The store's labels as listing rows (name, paper type, label size, columns, status, last change);
/// null <see cref="IsActive"/> returns all (065 FR-010). The full label is <c>GET api/labels/{id}</c>.
/// </summary>
public sealed record GetLabelsQuery(bool? IsActive) : IRequest<Result<IReadOnlyList<LabelSummaryResponse>>>;
