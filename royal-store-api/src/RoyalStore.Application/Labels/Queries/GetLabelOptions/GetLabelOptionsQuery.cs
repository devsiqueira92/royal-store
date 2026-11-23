using MediatR;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Queries.GetLabelOptions;

/// <summary>
/// The store's active labels for a selection list: <c>code</c> = label id, <c>description</c> =
/// label name; the default label first, then by name.
/// </summary>
public sealed record GetLabelOptionsQuery : IRequest<Result<IReadOnlyList<OptionResponse>>>;
