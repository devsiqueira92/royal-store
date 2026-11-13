using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoyalStore.Application.Abstractions;
using RoyalStore.Application.Options;

namespace RoyalStore.Infrastructure.Expenses;

/// <summary>
/// Registers the OPEX background generator. Program.cs stays clean — services arrive through
/// IInstallService discovery (Constitution VI).
/// </summary>
public sealed class ExpensesInstallService : IInstallService
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ExpenseRecurrenceOptions>(
            configuration.GetSection(ExpenseRecurrenceOptions.SectionName));

        services.AddHostedService<ExpenseRecurrenceWorker>();
    }
}
