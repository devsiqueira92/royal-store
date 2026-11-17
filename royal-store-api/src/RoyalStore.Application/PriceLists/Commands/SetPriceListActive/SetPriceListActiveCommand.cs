using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.SetPriceListActive;


/// <summary>
/// Activates or deactivates a price list. Deactivating keeps every product price on the list; the
/// list just stops being offered for pricing. The default list cannot be deactivated (061 FR-004).
/// </summary>
public record SetPriceListActiveCommand(Guid Id, bool IsActive) : IRequest<Result>;