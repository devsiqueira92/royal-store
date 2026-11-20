using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Commands.SetLabelActive;

/// <summary>
/// Activates or deactivates a label and its format (065 FR-014). Idempotent; deactivating the
/// default label also clears its default mark.
/// </summary>
public sealed record SetLabelActiveCommand(Guid Id, bool IsActive) : IRequest<Result>;
