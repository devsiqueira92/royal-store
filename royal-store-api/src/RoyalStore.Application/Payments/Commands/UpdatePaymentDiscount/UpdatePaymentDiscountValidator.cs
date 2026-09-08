using FluentValidation;

namespace RoyalStore.Application.Payments.Commands.UpdatePaymentDiscount;

public sealed class UpdatePaymentDiscountValidator : AbstractValidator<UpdatePaymentDiscountCommand>
{
    public UpdatePaymentDiscountValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Discount)
            .InclusiveBetween(0, 100).WithMessage("Discount must be between 0 and 100.");
    }
}
