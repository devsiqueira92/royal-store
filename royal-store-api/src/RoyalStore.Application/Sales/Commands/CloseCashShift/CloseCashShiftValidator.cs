using FluentValidation;

namespace RoyalStore.Application.Sales.Commands.CloseCashShift;

public sealed class CloseCashShiftValidator : AbstractValidator<CloseCashShiftCommand>
{
    public CloseCashShiftValidator()
    {
        RuleFor(x => x.ActualEndingCash)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Actual ending cash cannot be negative.");
    }
}
