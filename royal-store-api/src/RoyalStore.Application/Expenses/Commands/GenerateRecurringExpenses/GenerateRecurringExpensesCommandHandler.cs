using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.GenerateRecurringExpenses;

public sealed class GenerateRecurringExpensesCommandHandler
    : IRequestHandler<GenerateRecurringExpensesCommand, Result<RecurrenceGenerationResultResponse>>
{
    private readonly IExpenseRecurrenceRepository _recurrenceRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUniqueConstraintDetector _uniqueConstraintDetector;
    private readonly ILogger<GenerateRecurringExpensesCommandHandler> _logger;

    public GenerateRecurringExpensesCommandHandler(
        IExpenseRecurrenceRepository recurrenceRepository,
        IExpenseRepository expenseRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        IUniqueConstraintDetector uniqueConstraintDetector,
        ILogger<GenerateRecurringExpensesCommandHandler> logger)
    {
        _recurrenceRepository = recurrenceRepository;
        _expenseRepository = expenseRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _uniqueConstraintDetector = uniqueConstraintDetector;
        _logger = logger;
    }

    public async Task<Result<RecurrenceGenerationResultResponse>> Handle(
        GenerateRecurringExpensesCommand command, CancellationToken cancellationToken)
    {
        var today = command.Today ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var runAt = DateTimeOffset.UtcNow;
        var tenantId = _tenantContext.TenantId;

        var templates = await _recurrenceRepository.GetActiveAsync(cancellationToken);

        var created = 0;
        var skipped = 0;
        DateOnly? generatedThrough = null;

        foreach (var template in templates)
        {
            if (template.IsFinished(today))
                continue;

            // EnumerateDueCycles yields every cycle whose creation window has opened and that has
            // not been generated yet — so a worker that was down for a week finds several here and
            // catches up in one run (FR-033), with no special catch-up path to get wrong.
            foreach (var dueDate in template.EnumerateDueCycles(today))
            {
                var cycleKey = template.CycleKeyFor(dueDate);

                // Fast path only. The real guarantee is the unique filtered index on
                // (TenantId, ExpenseRecurrenceId, RecurrenceCycleKey): this check alone would lose
                // the race between check and insert when two runs overlap (research Decision 2).
                if (await _expenseRepository.ExistsForCycleAsync(template.Id, cycleKey, cancellationToken))
                {
                    skipped++;
                    template.RecordGeneration(dueDate);
                    continue;
                }

                var expenseResult = Expense.Create(
                    tenantId,
                    template.Description,
                    template.PayeeName,
                    template.SupplierId,
                    template.ExpenseCategoryId,
                    template.CostCenterId,
                    template.FinancialAccountId,
                    template.Amount,
                    new DateOnly(dueDate.Year, dueDate.Month, 1),
                    dueDate,
                    notes: null,
                    // A variable-amount template produces an estimate the manager replaces with the
                    // invoiced value when the bill arrives (FR-031).
                    isEstimated: template.IsVariableAmount,
                    expenseRecurrenceId: template.Id,
                    recurrenceCycleKey: cycleKey);

                if (expenseResult.IsFailure)
                {
                    // A template that cannot produce a valid bill must not abort the whole run for
                    // every other template; it is logged and skipped.
                    _logger.LogError(
                        "Recurring expense generation failed for template {RecurrenceId} cycle {CycleKey}: {ErrorCode} {ErrorMessage}.",
                        template.Id, cycleKey, expenseResult.Error.Code, expenseResult.Error.Message);

                    continue;
                }

                await _expenseRepository.AddAsync(expenseResult.Value, cancellationToken);

                try
                {
                    // Saved per cycle rather than once at the end: a unique-index collision on one
                    // cycle must not discard the bills already generated for other templates.
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    created++;
                    template.RecordGeneration(dueDate);

                    if (generatedThrough is null || dueDate > generatedThrough.Value)
                        generatedThrough = dueDate;
                }
                catch (DbUpdateException ex) when (_uniqueConstraintDetector.IsUniqueViolation(ex))
                {
                    // Another run created this cycle between the check above and this insert. That
                    // is the index doing its job, not a failure: the bill exists, which is the
                    // outcome we wanted (FR-032).
                    //
                    // The rejected row must be detached, or it stays Added and every later save in
                    // this run retries it — turning one lost race into a failed run.
                    _expenseRepository.Detach(expenseResult.Value);

                    skipped++;
                    template.RecordGeneration(dueDate);

                    _logger.LogInformation(
                        "Cycle {CycleKey} for template {RecurrenceId} was already generated by a concurrent run.",
                        cycleKey, template.Id);
                }
            }

            template.MarkRun(runAt);
        }

        // Persist the templates' progress markers and run timestamps.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Logged on EVERY run, including one that created nothing, so silence always means the
        // worker did not run rather than that it found nothing (Constitution VII, SC-011).
        _logger.LogInformation(
            "Recurring expense generation finished for tenant {TenantId}. Templates evaluated {TemplatesEvaluated}, expenses created {ExpensesCreated}, cycles skipped {CyclesSkipped}.",
            tenantId, templates.Count, created, skipped);

        return new RecurrenceGenerationResultResponse(
            templates.Count, created, skipped, generatedThrough, runAt);
    }
}
