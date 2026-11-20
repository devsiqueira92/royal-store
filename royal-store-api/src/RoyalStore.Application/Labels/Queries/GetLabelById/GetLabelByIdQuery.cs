using MediatR;
using RoyalStore.Application.Labels.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Queries.GetLabelById;

/// <summary>One label by its template id (065 FR-011).</summary>
public sealed record GetLabelByIdQuery(Guid Id) : IRequest<Result<LabelResponse>>;
