using System.Globalization;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Expenses;

/// <summary>
/// One operating obligation owed by a store — the aggregate root of this module. Owns its payments
/// and attachments, and is the single place the balance and status rules live (FR-016, FR-017).
/// </summary>
/// <remarks>
/// This type deliberately holds NO reference to CashShift, CashMovement, Sale, Product or any
/// inventory type. That absence is how FR-011 (operating cost only) and FR-019a (no cash-shift
/// coupling, clarified 2026-11-12) are enforced — by there being nothing to call, rather than by a
/// rule someone has to remember.
/// </remarks>
public sealed class Expense : Entity, IAuditableEntity
{
    public const int DescriptionMaxLength = 200;
    public const int PayeeNameMaxLength = 200;
    public const int NotesMaxLength = 1000;
    public const int RecurrenceCycleKeyMaxLength = 10;

    private readonly List<ExpensePayment> _payments = [];
    private readonly List<ExpenseAttachment> _attachments = [];

    public Guid TenantId { get; private set; }
    public string Description { get; private set; } = string.Empty;

    /// <summary>Denormalised so a supplier rename never rewrites history.</summary>
    /// <summary>Who is paid. Optional — null when the cost has no payee worth naming.</summary>
    public string? PayeeName { get; private set; }

    public Guid? SupplierId { get; private set; }
    public Guid ExpenseCategoryId { get; private set; }
    public Guid? CostCenterId { get; private set; }

    /// <summary>The account this bill is <em>expected</em> to be paid from; each payment names the account it actually left.</summary>
    public Guid? FinancialAccountId { get; private set; }

    public decimal Amount { get; private set; }

    /// <summary>True while the amount is a template estimate rather than an invoiced value (FR-004).</summary>
    public bool IsEstimated { get; private set; }

    /// <summary>Always the first day of the month — normalised by the factory (research Decision 4).</summary>
    public DateOnly CompetenceMonth { get; private set; }

    public DateOnly DueDate { get; private set; }
    public ExpenseStatus Status { get; private set; }
    public string? Notes { get; private set; }

    public Guid? ExpenseRecurrenceId { get; private set; }
    public string? RecurrenceCycleKey { get; private set; }

    /// <summary>
    /// Optimistic concurrency token, bumped by every state transition so two payments racing each
    /// other cannot both commit against a stale balance (FR-019). Same pattern as CashShift.Version.
    /// </summary>
    public int Version { get; private set; }

    public IReadOnlyList<ExpensePayment> Payments => _payments.AsReadOnly();
    public IReadOnlyList<ExpenseAttachment> Attachments => _attachments.AsReadOnly();

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Expense() : base(Guid.NewGuid()) { }

    private Expense(Guid id, Guid tenantId, string description, string? payeeName,
        Guid expenseCategoryId, decimal amount, DateOnly competenceMonth, DateOnly dueDate)
        : base(id)
    {
        TenantId = tenantId;
        Description = description;
        PayeeName = payeeName;
        ExpenseCategoryId = expenseCategoryId;
        Amount = amount;
        CompetenceMonth = competenceMonth;
        DueDate = dueDate;
        Status = ExpenseStatus.Pending;
    }

    // ---------------------------------------------------------------- derived values

    /// <summary>Total settled against this expense. Requires <see cref="Payments"/> to be loaded.</summary>
    public decimal SettledAmount => _payments.Where(p => !p.IsReversed).Sum(p => p.Amount);

    /// <summary>What is still owed (FR-017). Requires <see cref="Payments"/> to be loaded.</summary>
    public decimal RemainingBalance => Amount - SettledAmount;

    /// <summary>
    /// Derived at read time rather than stored (FR-020), which is what lets an expense be
    /// <see cref="ExpenseStatus.PartiallyPaid"/> and overdue at the same time.
    /// </summary>
    public bool IsOverdue(DateOnly today) =>
        DueDate < today && Status is not (ExpenseStatus.Paid or ExpenseStatus.Cancelled);

    public int DaysOverdue(DateOnly today) =>
        IsOverdue(today) ? today.DayNumber - DueDate.DayNumber : 0;

    public bool IsRecurring => ExpenseRecurrenceId is not null;

    // ---------------------------------------------------------------- factory

    private static Result ValidateFields(
        string description, string? payeeName, Guid expenseCategoryId, decimal amount, string? notes)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(Error.Validation("expense.description_required",
                "Expense description is required."));

        if (description.Trim().Length > DescriptionMaxLength)
            return Result.Failure(Error.Validation("expense.description_too_long",
                $"Expense description must not exceed {DescriptionMaxLength} characters."));

        // Optional: plenty of operating costs have no payee worth naming, and an
        // invented one is worse than none. Only its length is still policed.
        if (payeeName is not null && payeeName.Trim().Length > PayeeNameMaxLength)
            return Result.Failure(Error.Validation("expense.payee_too_long",
                $"Payee name must not exceed {PayeeNameMaxLength} characters."));

        if (expenseCategoryId == Guid.Empty)
            return Result.Failure(Error.Validation("expense.category_required",
                "Expense category is required."));

        if (amount <= 0)
            return Result.Failure(Error.Validation("expense.amount_invalid",
                "Expense amount must be greater than zero."));

        if (notes is not null && notes.Trim().Length > NotesMaxLength)
            return Result.Failure(Error.Validation("expense.notes_too_long",
                $"Expense notes must not exceed {NotesMaxLength} characters."));

        return Result.Success();
    }

    /// <summary>Normalises any date in the month to its first day, so grouping by competence month is reliable.</summary>
    private static DateOnly NormalizeCompetenceMonth(DateOnly value) =>
        new(value.Year, value.Month, 1);

    /// <summary>Blank becomes null, so an empty string never passes for a value that was never given.</summary>
    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static Result<Expense> Create(
        Guid tenantId, string description, string? payeeName, Guid? supplierId,
        Guid expenseCategoryId, Guid? costCenterId, Guid? financialAccountId,
        decimal amount, DateOnly competenceMonth, DateOnly dueDate,
        string? notes, bool isEstimated,
        Guid? expenseRecurrenceId = null, string? recurrenceCycleKey = null)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("expense.tenant_required", "Tenant identifier is required.");

        var validation = ValidateFields(description, payeeName, expenseCategoryId, amount, notes);
        if (validation.IsFailure)
            return validation.Error;

        if (recurrenceCycleKey is not null && recurrenceCycleKey.Length > RecurrenceCycleKeyMaxLength)
            return Error.Validation("expense.cycle_key_too_long",
                $"Recurrence cycle key must not exceed {RecurrenceCycleKeyMaxLength} characters.");

        return new Expense(Guid.NewGuid(), tenantId, description.Trim(), NormalizeOptional(payeeName),
            expenseCategoryId, amount, NormalizeCompetenceMonth(competenceMonth), dueDate)
        {
            SupplierId = supplierId,
            CostCenterId = costCenterId,
            FinancialAccountId = financialAccountId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            IsEstimated = isEstimated,
            ExpenseRecurrenceId = expenseRecurrenceId,
            RecurrenceCycleKey = recurrenceCycleKey,
        };
    }

    // ---------------------------------------------------------------- state transitions

    /// <summary>
    /// Registers a settlement (FR-012–FR-016). Every rejection is a business rule returned as a
    /// Result, never an exception.
    /// </summary>
    /// <param name="today">The caller's current date, used to reject a future payment (FR-014).</param>
    public Result<ExpensePayment> RegisterPayment(
        Guid financialAccountId, decimal amount, DateOnly paidAt,
        string? methodNote, string? notes, DateOnly today)
    {
        if (Status == ExpenseStatus.Cancelled)
            return Error.Conflict("expense.cancelled",
                "A cancelled expense cannot receive payments.");

        if (amount <= 0)
            return Error.Validation("expensepayment.amount_invalid",
                "Payment amount must be greater than zero.");

        if (paidAt > today)
            return Error.Validation("expensepayment.future_date",
                "A payment cannot be dated in the future.");

        if (amount > RemainingBalance)
            return Error.Conflict("expensepayment.exceeds_balance",
                $"Payment exceeds the remaining balance ({RemainingBalance.ToString("F2", CultureInfo.InvariantCulture)}).");

        var paymentResult = ExpensePayment.Create(
            TenantId, Id, financialAccountId, amount, paidAt, methodNote, notes);

        if (paymentResult.IsFailure)
            return paymentResult.Error;

        _payments.Add(paymentResult.Value);
        RecomputeStatus();
        Version++;

        return paymentResult.Value;
    }

    /// <summary>Reverses a payment and recalculates the balance and status (FR-018).</summary>
    public Result ReversePayment(Guid paymentId, DateTimeOffset reversedAt, string? reason)
    {
        var payment = _payments.FirstOrDefault(p => p.Id == paymentId);
        if (payment is null)
            return Result.Failure(Error.NotFound("expensepayment.not_found",
                "Payment was not found on this expense."));

        var reversal = payment.Reverse(reversedAt, reason);
        if (reversal.IsFailure)
            return reversal;

        RecomputeStatus();
        Version++;

        return Result.Success();
    }

    /// <summary>Cancels an expense recorded by mistake (FR-007).</summary>
    public Result Cancel()
    {
        if (Status == ExpenseStatus.Cancelled)
            return Result.Failure(Error.Conflict("expense.already_cancelled",
                "Expense is already cancelled."));

        if (Status == ExpenseStatus.Paid)
            return Result.Failure(Error.Conflict("expense.already_paid",
                "A fully paid expense cannot be cancelled."));

        if (_payments.Any(p => !p.IsReversed))
            return Result.Failure(Error.Conflict("expense.has_payments",
                "Reverse this expense's payments before cancelling it."));

        Status = ExpenseStatus.Cancelled;
        Version++;

        return Result.Success();
    }

    /// <summary>
    /// Changes the amount, refusing to drop it below what is already settled (FR-006), and clearing
    /// the estimate flag because a hand-entered amount is an invoiced value (FR-004).
    /// </summary>
    public Result ChangeAmount(decimal newAmount)
    {
        if (Status == ExpenseStatus.Cancelled)
            return Result.Failure(Error.Conflict("expense.cancelled",
                "A cancelled expense cannot be edited."));

        if (newAmount <= 0)
            return Result.Failure(Error.Validation("expense.amount_invalid",
                "Expense amount must be greater than zero."));

        if (newAmount < SettledAmount)
            return Result.Failure(Error.Conflict("expense.amount_below_settled",
                $"Amount cannot be lower than the {SettledAmount.ToString("F2", CultureInfo.InvariantCulture)} already settled."));

        Amount = newAmount;
        IsEstimated = false;
        RecomputeStatus();
        Version++;

        return Result.Success();
    }

    /// <summary>Edits the descriptive and classification fields (FR-005). Amount goes through <see cref="ChangeAmount"/>.</summary>
    public Result UpdateDetails(
        string description, string? payeeName, Guid? supplierId,
        Guid expenseCategoryId, Guid? costCenterId, Guid? financialAccountId,
        DateOnly competenceMonth, DateOnly dueDate, string? notes)
    {
        if (Status == ExpenseStatus.Cancelled)
            return Result.Failure(Error.Conflict("expense.cancelled",
                "A cancelled expense cannot be edited."));

        var validation = ValidateFields(description, payeeName, expenseCategoryId, Amount, notes);
        if (validation.IsFailure)
            return validation;

        Description = description.Trim();
        PayeeName = NormalizeOptional(payeeName);
        SupplierId = supplierId;
        ExpenseCategoryId = expenseCategoryId;
        CostCenterId = costCenterId;
        FinancialAccountId = financialAccountId;
        CompetenceMonth = NormalizeCompetenceMonth(competenceMonth);
        DueDate = dueDate;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Version++;

        return Result.Success();
    }

    /// <summary>
    /// The single place <see cref="Status"/> is ever assigned outside cancellation (FR-016).
    /// Never produces <see cref="ExpenseStatus.Cancelled"/>, which only <see cref="Cancel"/> sets.
    /// </summary>
    private void RecomputeStatus()
    {
        if (Status == ExpenseStatus.Cancelled)
            return;

        var settled = SettledAmount;

        Status = settled switch
        {
            0 => ExpenseStatus.Pending,
            _ when settled >= Amount => ExpenseStatus.Paid,
            _ => ExpenseStatus.PartiallyPaid,
        };
    }
}
