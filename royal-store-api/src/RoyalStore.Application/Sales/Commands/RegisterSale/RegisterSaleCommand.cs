using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Commands.RegisterSale;

/// <summary>
/// Records one completed point-of-sale checkout. The terminal states what was sold, how it was paid
/// and the totals it presented; everything else — naming, pricing and fiscal classification — is
/// resolved server-side from the catalog and the published tax table.
/// </summary>
public record RegisterSaleCommand(
    RegisterSaleMetadata Metadata,
    RegisterSaleCustomer? Customer,
    IReadOnlyList<RegisterSaleItem> Items,
    IReadOnlyList<RegisterSalePayment> Payments,
    RegisterSaleTotals Sale) : IRequest<Result<RegisterSaleResponse>>;
