using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Expenses.Commands.GenerateRecurringExpenses;
using RoyalStore.Application.Options;

namespace RoyalStore.Infrastructure.Expenses;

/// <summary>
/// Creates each store's recurring bills ahead of their due dates (FR-031), once a day.
/// </summary>
/// <remarks>
/// <para>
/// The tenant problem this solves: a BackgroundService has no HttpContext, so the JWT-backed tenant
/// context would yield Guid.Empty and every global query filter would match nothing — the worker
/// would find no templates and create no bills, while reporting success. Each tenant is therefore
/// run inside an <see cref="ITenantScope"/>, and the DI scope (and with it the DbContext) is
/// created INSIDE that scope, because EF evaluates the filter against the injected ITenantContext
/// per query (research Decision 1).
/// </para>
/// <para>
/// Catch-up needs no special handling here: each run asks every template for all cycles whose
/// creation window has opened and that have no bill yet, so a worker that was down for a week
/// simply finds several per template on its next run (FR-033).
/// </para>
/// </remarks>
public sealed class ExpenseRecurrenceWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITenantScope _tenantScope;
    private readonly IOptions<ExpenseRecurrenceOptions> _options;
    private readonly ILogger<ExpenseRecurrenceWorker> _logger;

    private DateOnly? _lastRunDate;

    public ExpenseRecurrenceWorker(
        IServiceScopeFactory scopeFactory,
        ITenantScope tenantScope,
        IOptions<ExpenseRecurrenceOptions> options,
        ILogger<ExpenseRecurrenceWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _tenantScope = tenantScope;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _options.Value;

        if (!options.Enabled)
        {
            _logger.LogInformation(
                "Expense recurrence worker is disabled by configuration; no scheduled generation will run.");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(options.PollIntervalMinutes));

        _logger.LogInformation(
            "Expense recurrence worker started. Daily run at {RunHourUtc}:00 UTC, polling every {PollIntervalMinutes} minutes.",
            options.RunHourUtc, options.PollIntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var today = DateOnly.FromDateTime(now);

                // One run per day, on or after the configured hour. Tracking the last run date
                // rather than sleeping until the exact moment means a restart mid-day still runs
                // today's generation instead of skipping it.
                if (now.Hour >= options.RunHourUtc && _lastRunDate != today)
                {
                    await RunForAllTenantsAsync(today, stoppingToken);
                    _lastRunDate = today;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // A failed run must never kill the worker: tomorrow's bills matter more than this
                // run, and the failure is visible in the log.
                _logger.LogError(ex, "Expense recurrence run failed. The worker will try again on the next cycle.");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunForAllTenantsAsync(DateOnly today, CancellationToken ct)
    {
        var tenantIds = await GetTenantIdsAsync(ct);

        var tenantsWithWork = 0;
        var totalCreated = 0;
        var totalSkipped = 0;

        foreach (var tenantId in tenantIds)
        {
            if (ct.IsCancellationRequested)
                break;

            try
            {
                // Order matters: the ambient tenant must be set BEFORE the DI scope is created, so
                // the DbContext resolved inside it filters on this tenant.
                using var tenantScope = _tenantScope.Begin(tenantId);
                await using var serviceScope = _scopeFactory.CreateAsyncScope();

                var mediator = serviceScope.ServiceProvider.GetRequiredService<IMediator>();
                var result = await mediator.Send(new GenerateRecurringExpensesCommand(today), ct);

                if (result.IsFailure)
                {
                    _logger.LogError(
                        "Expense generation failed for tenant {TenantId}: {ErrorCode} {ErrorMessage}.",
                        tenantId, result.Error.Code, result.Error.Message);

                    continue;
                }

                totalCreated += result.Value.ExpensesCreated;
                totalSkipped += result.Value.CyclesSkipped;

                if (result.Value.ExpensesCreated > 0)
                    tenantsWithWork++;
            }
            catch (Exception ex)
            {
                // One tenant's failure must not stop the others from getting their bills.
                _logger.LogError(ex, "Expense generation threw for tenant {TenantId}. Continuing.", tenantId);
            }
        }

        // Emitted on EVERY run, including one that created nothing — Constitution VII requires that
        // of background services so silence means "did not run", never "ran and found nothing".
        _logger.LogInformation(
            "Expense recurrence run complete for {RunDate}. Tenants evaluated {TenantsEvaluated}, tenants with new bills {TenantsWithNewBills}, expenses created {ExpensesCreated}, cycles skipped {CyclesSkipped}.",
            today, tenantIds.Count, tenantsWithWork, totalCreated, totalSkipped);
    }

    /// <summary>
    /// Reads every tenant. Tenants carry no global query filter, so this needs no ambient scope and
    /// no filter bypass.
    /// </summary>
    private async Task<IReadOnlyList<Guid>> GetTenantIdsAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IExpenseRecurrenceRepository>();
        return await repository.GetAllTenantIdsAsync(ct);
    }
}
