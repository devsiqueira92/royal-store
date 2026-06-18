using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Fiscal;
using RoyalStore.Application.Fiscal.Commands.ProcessFiscalInvoice;

namespace RoyalStore.Infrastructure.Fiscal;

public sealed class FiscalIntegrationWorker : BackgroundService
{
    private readonly IFiscalReceiptQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FiscalIntegrationWorker> _logger;

    public FiscalIntegrationWorker(
        IFiscalReceiptQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<FiscalIntegrationWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var saleId in _queue.DequeueAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new ProcessFiscalInvoiceCommand(saleId), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing fiscal invoice for sale {SaleId}. Continuing.", saleId);
            }
        }
    }
}
