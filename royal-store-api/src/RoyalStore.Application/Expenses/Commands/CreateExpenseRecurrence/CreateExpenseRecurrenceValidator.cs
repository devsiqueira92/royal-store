using FluentValidation;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.CreateExpenseRecurrence;

public sealed class CreateExpenseRecurrenceValidator : AbstractValidator<CreateExpenseRecurrenceCommand>
{
    public CreateExpenseRecurrenceValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().MaximumLength(ExpenseRecurrence.DescriptionMaxLength);

        // Optional — only its length is checked.
        RuleFor(x => x.PayeeName)
            .MaximumLength(ExpenseRecurrence.PayeeNameMaxLength);

        RuleFor(x => x.ExpenseCategoryId).NotEmpty();

        RuleFor(x => x.Frequency)
            .Must(frequency => ExpenseRecurrenceResponse.ParseFrequency(frequency) is not null)
            .WithMessage($"Frequency must be one of: {ExpenseRecurrenceResponse.FrequencyNames}.");

        // Weekly templates use an ISO weekday (1-7); everything else a day of the month (1-31),
        // clamped to the month's length at generation time (FR-034). Both rules sit out an
        // unparseable frequency, so the response names that one problem instead of adding a
        // day-range message that depends on it.
        RuleFor(x => x.DueDay)
            .InclusiveBetween(1, 7)
            .When(x => x.ParsedFrequency == RecurrenceFrequency.Weekly)
            .WithMessage("Due day must be between 1 and 7 for a weekly template.");

        RuleFor(x => x.DueDay)
            .InclusiveBetween(1, 31)
            .When(x => x.ParsedFrequency is { } frequency && frequency != RecurrenceFrequency.Weekly)
            .WithMessage("Due day must be between 1 and 31.");

        RuleFor(x => x.LeadDays)
            .InclusiveBetween(0, ExpenseRecurrence.LeadDaysMax)
            .WithMessage($"Lead days must be between 0 and {ExpenseRecurrence.LeadDaysMax}.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .PrecisionScale(18, 2, true)
            .WithMessage("Template amount must have at most two decimal places.");

        RuleFor(x => x.MaxOccurrences)
            .GreaterThan(0).When(x => x.MaxOccurrences.HasValue);
    }
}
