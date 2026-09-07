using FluentValidation;

namespace RoyalStore.Application.Loyalty.Commands.CreateLoyaltyTier;

public sealed class CreateLoyaltyTierValidator : AbstractValidator<CreateLoyaltyTierCommand>
{
    public CreateLoyaltyTierValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tier name is required.")
            .MaximumLength(50).WithMessage("Tier name must not exceed 50 characters.");

        RuleFor(x => x.CashbackPercentage)
            .InclusiveBetween(0, 100).WithMessage("Cashback percentage must be between 0 and 100.");

        RuleFor(x => x.CashbackValidityDays)
            .GreaterThan(0).WithMessage("Cashback validity period must be greater than zero days.");
    }
}
