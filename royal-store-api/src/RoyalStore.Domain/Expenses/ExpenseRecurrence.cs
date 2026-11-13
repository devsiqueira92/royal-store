using System.Globalization;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Expenses;

/// <summary>
/// A template that generates operating expenses on a cycle (FR-030). Holds everything a generated
/// bill needs, so generation copies values rather than referencing them — which is what makes a
/// template edit affect only future cycles (FR-038).
/// </summary>
public sealed class ExpenseRecurrence : Entity, IAuditableEntity
{
    public const int DescriptionMaxLength = 200;
    public const int PayeeNameMaxLength = 200;
    public const int LeadDaysMax = 60;

    /// <summary>Hard stop on cycle enumeration so a misconfigured template can never spin forever.</summary>
    private const int MaxCyclesScanned = 2000;

    public Guid TenantId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    /// <summary>Who is paid. Optional, as on <see cref="Expense.PayeeName"/>.</summary>
    public string? PayeeName { get; private set; }
    public Guid? SupplierId { get; private set; }
    public Guid ExpenseCategoryId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public Guid? FinancialAccountId { get; private set; }

    public RecurrenceFrequency Frequency { get; private set; }

    /// <summary>Day of the cycle the bill falls due: 1–31 for monthly and longer, 1–7 (ISO weekday) for weekly.</summary>
    public int DueDay { get; private set; }

    /// <summary>How many days ahead of the due date the bill is created (FR-030).</summary>
    public int LeadDays { get; private set; }

    public decimal Amount { get; private set; }
    public bool IsVariableAmount { get; private set; }
    public bool IsActive { get; private set; }

    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public int? MaxOccurrences { get; private set; }
    public int OccurrencesGenerated { get; private set; }

    /// <summary>
    /// Progress marker for reporting (FR-036). Never the correctness mechanism — the unique index on
    /// (TenantId, ExpenseRecurrenceId, RecurrenceCycleKey) is what prevents duplicates
    /// (research Decision 2), so a stale value after a crash still cannot double-insert.
    /// </summary>
    public DateOnly? GeneratedThrough { get; private set; }

    public DateTimeOffset? LastRunAt { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private ExpenseRecurrence() : base(Guid.NewGuid()) { }

    private ExpenseRecurrence(Guid id, Guid tenantId, string description, string? payeeName,
        Guid expenseCategoryId, RecurrenceFrequency frequency, int dueDay, int leadDays,
        decimal amount, DateOnly startDate)
        : base(id)
    {
        TenantId = tenantId;
        Description = description;
        PayeeName = payeeName;
        ExpenseCategoryId = expenseCategoryId;
        Frequency = frequency;
        DueDay = dueDay;
        LeadDays = leadDays;
        Amount = amount;
        StartDate = startDate;
        IsActive = true;
    }

    /// <summary>Blank becomes null, so an empty string never passes for a value that was never given.</summary>
    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Result ValidateFields(
        string description, string? payeeName, Guid expenseCategoryId, RecurrenceFrequency frequency,
        int dueDay, int leadDays, decimal amount, DateOnly startDate, DateOnly? endDate, int? maxOccurrences)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(Error.Validation("expenserecurrence.description_required",
                "Template description is required."));

        if (description.Trim().Length > DescriptionMaxLength)
            return Result.Failure(Error.Validation("expenserecurrence.description_too_long",
                $"Template description must not exceed {DescriptionMaxLength} characters."));

        // Optional, like the expense it generates; only its length is policed.
        if (payeeName is not null && payeeName.Trim().Length > PayeeNameMaxLength)
            return Result.Failure(Error.Validation("expenserecurrence.payee_too_long",
                $"Payee name must not exceed {PayeeNameMaxLength} characters."));

        if (expenseCategoryId == Guid.Empty)
            return Result.Failure(Error.Validation("expenserecurrence.category_required",
                "Expense category is required."));

        if (!Enum.IsDefined(frequency))
            return Result.Failure(Error.Validation("expenserecurrence.frequency_invalid",
                "Recurrence frequency is invalid."));

        var maxDueDay = frequency == RecurrenceFrequency.Weekly ? 7 : 31;
        if (dueDay < 1 || dueDay > maxDueDay)
            return Result.Failure(Error.Validation("expenserecurrence.due_day_invalid",
                $"Due day must be between 1 and {maxDueDay} for this frequency."));

        if (leadDays < 0 || leadDays > LeadDaysMax)
            return Result.Failure(Error.Validation("expenserecurrence.lead_days_invalid",
                $"Lead days must be between 0 and {LeadDaysMax}."));

        if (amount <= 0)
            return Result.Failure(Error.Validation("expenserecurrence.amount_invalid",
                "Template amount must be greater than zero."));

        if (endDate is not null && endDate.Value < startDate)
            return Result.Failure(Error.Validation("expenserecurrence.end_before_start",
                "End date cannot be earlier than the start date."));

        if (maxOccurrences is not null && maxOccurrences.Value <= 0)
            return Result.Failure(Error.Validation("expenserecurrence.max_occurrences_invalid",
                "Maximum occurrences must be greater than zero."));

        return Result.Success();
    }

    public static Result<ExpenseRecurrence> Create(
        Guid tenantId, string description, string? payeeName, Guid? supplierId,
        Guid expenseCategoryId, Guid? costCenterId, Guid? financialAccountId,
        RecurrenceFrequency frequency, int dueDay, int leadDays,
        decimal amount, bool isVariableAmount,
        DateOnly startDate, DateOnly? endDate, int? maxOccurrences)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("expenserecurrence.tenant_required", "Tenant identifier is required.");

        var validation = ValidateFields(description, payeeName, expenseCategoryId, frequency,
            dueDay, leadDays, amount, startDate, endDate, maxOccurrences);
        if (validation.IsFailure)
            return validation.Error;

        return new ExpenseRecurrence(Guid.NewGuid(), tenantId, description.Trim(), NormalizeOptional(payeeName),
            expenseCategoryId, frequency, dueDay, leadDays, amount, startDate)
        {
            SupplierId = supplierId,
            CostCenterId = costCenterId,
            FinancialAccountId = financialAccountId,
            IsVariableAmount = isVariableAmount,
            EndDate = endDate,
            MaxOccurrences = maxOccurrences,
        };
    }

    /// <summary>
    /// Edits the template. Bills already generated are untouched (FR-038) because generation copied
    /// these values onto them rather than referencing the template.
    /// </summary>
    public Result Update(
        string description, string? payeeName, Guid? supplierId,
        Guid expenseCategoryId, Guid? costCenterId, Guid? financialAccountId,
        RecurrenceFrequency frequency, int dueDay, int leadDays,
        decimal amount, bool isVariableAmount,
        DateOnly startDate, DateOnly? endDate, int? maxOccurrences)
    {
        var validation = ValidateFields(description, payeeName, expenseCategoryId, frequency,
            dueDay, leadDays, amount, startDate, endDate, maxOccurrences);
        if (validation.IsFailure)
            return validation;

        Description = description.Trim();
        PayeeName = NormalizeOptional(payeeName);
        SupplierId = supplierId;
        ExpenseCategoryId = expenseCategoryId;
        CostCenterId = costCenterId;
        FinancialAccountId = financialAccountId;
        Frequency = frequency;
        DueDay = dueDay;
        LeadDays = leadDays;
        Amount = amount;
        IsVariableAmount = isVariableAmount;
        StartDate = startDate;
        EndDate = endDate;
        MaxOccurrences = maxOccurrences;

        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(Error.Conflict("expenserecurrence.already_active",
                "Template is already active."));

        IsActive = true;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("expenserecurrence.already_inactive",
                "Template is already inactive."));

        IsActive = false;
        return Result.Success();
    }

    /// <summary>End date passed or occurrence limit reached — generation stops either way (FR-035).</summary>
    public bool IsFinished(DateOnly today) =>
        (EndDate is not null && EndDate.Value < today) ||
        (MaxOccurrences is not null && OccurrencesGenerated >= MaxOccurrences.Value);

    /// <summary>
    /// The due date of cycle <paramref name="cycleIndex"/> counted from <see cref="StartDate"/>.
    /// For monthly and longer frequencies the day is clamped to the last day of the target month, so
    /// a day-31 template lands on 28/29 February rather than failing or spilling into March (FR-034).
    /// </summary>
    public DateOnly CalculateDueDate(int cycleIndex)
    {
        if (Frequency == RecurrenceFrequency.Weekly)
        {
            // Monday of the week containing StartDate, then the requested ISO weekday.
            var offsetToMonday = ((int)StartDate.DayOfWeek + 6) % 7;
            var anchorMonday = StartDate.AddDays(-offsetToMonday);
            return anchorMonday.AddDays((7 * cycleIndex) + (DueDay - 1));
        }

        var monthStep = Frequency switch
        {
            RecurrenceFrequency.Monthly => 1,
            RecurrenceFrequency.Quarterly => 3,
            RecurrenceFrequency.Semiannually => 6,
            RecurrenceFrequency.Yearly => 12,
            _ => 1,
        };

        var anchorMonth = new DateOnly(StartDate.Year, StartDate.Month, 1).AddMonths(monthStep * cycleIndex);
        var daysInMonth = DateTime.DaysInMonth(anchorMonth.Year, anchorMonth.Month);
        return new DateOnly(anchorMonth.Year, anchorMonth.Month, Math.Min(DueDay, daysInMonth));
    }

    /// <summary>
    /// The canonical identity of the cycle a due date belongs to: <c>"2026-12"</c>, or
    /// <c>"2026-W49"</c> when weekly. Stored on the generated expense, where it serves both the
    /// uniqueness guarantee (FR-032) and the projection's committed/projected de-duplication
    /// (FR-046) — one key, so the two can never disagree.
    /// </summary>
    public string CycleKeyFor(DateOnly dueDate)
    {
        if (Frequency != RecurrenceFrequency.Weekly)
            return dueDate.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        var asDateTime = dueDate.ToDateTime(TimeOnly.MinValue);
        var isoYear = ISOWeek.GetYear(asDateTime);
        var isoWeek = ISOWeek.GetWeekOfYear(asDateTime);
        return string.Create(CultureInfo.InvariantCulture, $"{isoYear:D4}-W{isoWeek:D2}");
    }

    /// <summary>
    /// Every cycle due date whose creation window has opened and that has not been generated yet.
    /// A single method serves normal generation (FR-031), catch-up after missed runs (FR-033) and
    /// the stop conditions (FR-035): a worker that was down for a week simply finds several cycles
    /// here on its next run.
    /// </summary>
    public IEnumerable<DateOnly> EnumerateDueCycles(DateOnly today)
    {
        if (!IsActive)
            yield break;

        var remaining = MaxOccurrences is null
            ? int.MaxValue
            : MaxOccurrences.Value - OccurrencesGenerated;

        if (remaining <= 0)
            yield break;

        var produced = 0;

        for (var cycleIndex = 0; cycleIndex < MaxCyclesScanned; cycleIndex++)
        {
            var dueDate = CalculateDueDate(cycleIndex);

            // Cycles before the template started, and ones already generated, are skipped rather
            // than ending the scan — the first cycle of a mid-month start can precede StartDate.
            if (dueDate < StartDate)
                continue;

            if (GeneratedThrough is not null && dueDate <= GeneratedThrough.Value)
                continue;

            if (EndDate is not null && dueDate > EndDate.Value)
                yield break;

            // The creation window has not opened yet; every later cycle is further out still.
            if (dueDate.AddDays(-LeadDays) > today)
                yield break;

            yield return dueDate;

            if (++produced >= remaining)
                yield break;
        }
    }

    /// <summary>The due date of the next bill this template will produce, or null once finished.</summary>
    public DateOnly? NextBillDate()
    {
        for (var cycleIndex = 0; cycleIndex < MaxCyclesScanned; cycleIndex++)
        {
            var dueDate = CalculateDueDate(cycleIndex);

            if (dueDate < StartDate)
                continue;

            if (GeneratedThrough is not null && dueDate <= GeneratedThrough.Value)
                continue;

            if (EndDate is not null && dueDate > EndDate.Value)
                return null;

            if (MaxOccurrences is not null && OccurrencesGenerated >= MaxOccurrences.Value)
                return null;

            return dueDate;
        }

        return null;
    }

    /// <summary>Records that a bill was created for <paramref name="dueDate"/>.</summary>
    public void RecordGeneration(DateOnly dueDate)
    {
        if (GeneratedThrough is null || dueDate > GeneratedThrough.Value)
            GeneratedThrough = dueDate;

        OccurrencesGenerated++;
    }

    public void MarkRun(DateTimeOffset runAt) => LastRunAt = runAt;
}
