using RoyalStore.Application.Abstractions.Fiscal;
using RoyalStore.Application.Fiscal;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Infrastructure.Fiscal;

public sealed class MockFiscalApiService : IFiscalApiService
{
    public async Task<FiscalResult> EmitInvoiceAsync(Sale sale, CancellationToken ct)
    {
        await Task.Delay(2000, ct);
        return FiscalResult.Success($"https://mock.fiscal.api/receipt/{sale.Id}");
    }
}
