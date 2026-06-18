namespace RoyalStore.Application.Abstractions.Fiscal;

public interface IFiscalReceiptQueue
{
    ValueTask QueueSaleAsync(Guid saleId, CancellationToken ct = default);
    IAsyncEnumerable<Guid> DequeueAsync(CancellationToken ct);
}
