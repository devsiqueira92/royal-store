using FluentValidation;

namespace RoyalStore.Application.Loyalty.Commands.UpdateLoyaltyTier;

public sealed class UpdateLoyaltyTierValidator : AbstractValidator<UpdateLoyaltyTierCommand>
{
    public UpdateLoyaltyTierValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.CashbackPercentage)
            .InclusiveBetween(0, 100).WithMessage("Cashback percentage must be between 0 and 100.");

        RuleFor(x => x.CashbackValidityDays)
            .GreaterThan(0).WithMessage("Cashback validity period must be greater than zero days.");
    }
}
