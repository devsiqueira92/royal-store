using RoyalStore.Application.Abstractions.Fiscal;
using System.Threading.Channels;

namespace RoyalStore.Infrastructure.Fiscal;

public sealed class FiscalReceiptQueue : IFiscalReceiptQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public ValueTask QueueSaleAsync(Guid saleId, CancellationToken ct = default)
    {
        _channel.Writer.TryWrite(saleId);
        return ValueTask.CompletedTask;
    }

    public async IAsyncEnumerable<Guid> DequeueAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var id in _channel.Reader.ReadAllAsync(ct))
            yield return id;
    }
}
