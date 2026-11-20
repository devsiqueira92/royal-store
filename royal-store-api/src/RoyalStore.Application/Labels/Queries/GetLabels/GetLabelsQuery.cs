using MediatR;
using RoyalStore.Application.Labels.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Queries.GetLabels;

/// <summary>The store's labels with their formats; null <see cref="IsActive"/> returns all (065 FR-010).</summary>
public sealed record GetLabelsQuery(bool? IsActive) : IRequest<Result<IReadOnlyList<LabelResponse>>>;
