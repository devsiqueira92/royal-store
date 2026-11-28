using MediatR;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Queries.GetLoyaltyTierOptions;

/// <summary>The store's active tiers, shaped for a dropdown: <c>Code</c> is the tier id,
/// <c>Description</c> its name.</summary>
public record GetLoyaltyTierOptionsQuery : IRequest<Result<IReadOnlyList<OptionResponse>>>;
