using FluentValidation;

namespace RoyalStore.Application.Sales.Commands.OpenCashShift;

public sealed class OpenCashShiftValidator : AbstractValidator<OpenCashShiftCommand>
{
    public OpenCashShiftValidator()
    {
        RuleFor(x => x.StartingCash)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Starting cash cannot be negative.");
    }
}
