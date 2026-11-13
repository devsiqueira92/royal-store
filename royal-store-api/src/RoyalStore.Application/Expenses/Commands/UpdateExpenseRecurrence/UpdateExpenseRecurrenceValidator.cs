using FluentValidation;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.UpdateExpenseRecurrence;

public sealed class UpdateExpenseRecurrenceValidator : AbstractValidator<UpdateExpenseRecurrenceCommand>
{
    public UpdateExpenseRecurrenceValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty().MaximumLength(ExpenseRecurrence.DescriptionMaxLength);

        // Optional — only its length is checked.
        RuleFor(x => x.PayeeName)
            .MaximumLength(ExpenseRecurrence.PayeeNameMaxLength);

        RuleFor(x => x.ExpenseCategoryId).NotEmpty();

        RuleFor(x => x.Frequency)
            .Must(frequency => ExpenseRecurrenceResponse.ParseFrequency(frequency) is not null)
            .WithMessage($"Frequency must be one of: {ExpenseRecurrenceResponse.FrequencyNames}.");

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
            .PrecisionScale(18, 2, true);

        RuleFor(x => x.MaxOccurrences)
            .GreaterThan(0).When(x => x.MaxOccurrences.HasValue);
    }
}
