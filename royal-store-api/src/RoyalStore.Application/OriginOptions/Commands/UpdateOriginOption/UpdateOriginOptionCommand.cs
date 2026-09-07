using MediatR;
using RoyalStore.Application.OriginOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.OriginOptions.Commands.UpdateOriginOption;

/// <summary>
/// <see cref="CurrentCode"/> comes from the route (addresses the entry to update).
/// <see cref="Code"/> comes from the request body — it may rename the entry to a new code,
/// which is still subject to the platform-wide uniqueness rule.
/// </summary>
public record UpdateOriginOptionCommand(string CurrentCode, string Code, string Name, string Description)
    : IRequest<Result<OriginOptionResponse>>;
