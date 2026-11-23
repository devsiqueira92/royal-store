using MediatR;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Queries.GetLabelPrintOptions;

/// <summary>
/// The labels a print can use: the store's active labels (default first, then by name), then the system
/// configurations by name. <c>code</c> = label id (open it with <c>GET api/labels/{id}</c>),
/// <c>description</c> = name.
/// </summary>
public sealed record GetLabelPrintOptionsQuery : IRequest<Result<IReadOnlyList<OptionResponse>>>;
