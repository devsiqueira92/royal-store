using FluentValidation;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Commands.RecordCashMovement;

public sealed class RecordCashMovementValidator : AbstractValidator<RecordCashMovementCommand>
{
    public RecordCashMovementValidator()
    {
        // Only supplements and withdrawals can be recorded by hand; Sale movements come from sale registration.
        RuleFor(x => x.Type)
            .Must(t => t is MovementType.In or MovementType.Out)
            .WithMessage("Movement type is invalid.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Movement amount must be greater than zero.")
            .PrecisionScale(18, 2, true)
            .WithMessage("Movement amount must have at most two decimal places.");

        RuleFor(x => x.Description)
            .MaximumLength(CashMovement.DescriptionMaxLength)
            .WithMessage("Movement description must not exceed 250 characters.");
    }
}
