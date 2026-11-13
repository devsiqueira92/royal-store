using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoyalStore.Application.Abstractions;
using RoyalStore.Application.Behaviors;

namespace RoyalStore.Application;

public sealed class DependencyInjection : IInstallService
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(AssemblyReference).Assembly, includeInternalTypes: true);

        // Shared reference check for the OPEX module — one statement of the rule for the create,
        // update and recurrence handlers (060 FR-028).
        services.AddScoped<Expenses.Common.ExpenseClassificationGuard>();
    }
}
