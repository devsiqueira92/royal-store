using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Commands.DeleteLabel;

/// <summary>Removes a label and its format permanently (065 FR-015).</summary>
public sealed record DeleteLabelCommand(Guid Id) : IRequest<Result>;
