using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoyalStore.Application.Abstractions;
using RoyalStore.Application.Abstractions.Fiscal;

namespace RoyalStore.Infrastructure.Fiscal;

public sealed class FiscalInstallService : IInstallService
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IFiscalReceiptQueue, FiscalReceiptQueue>();
        services.AddHostedService<FiscalIntegrationWorker>();
        services.AddScoped<IFiscalApiService, MockFiscalApiService>();
    }
}
