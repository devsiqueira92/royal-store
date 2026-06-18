using RoyalStore.Application.Fiscal;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Abstractions.Fiscal;

public interface IFiscalApiService
{
    Task<FiscalResult> EmitInvoiceAsync(Sale sale, CancellationToken ct);
}
