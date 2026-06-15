using FluentValidation;

namespace RoyalStore.Application.Sellers.Commands.UpdateSeller;

public sealed class UpdateSellerValidator : AbstractValidator<UpdateSellerCommand>
{
    public UpdateSellerValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithErrorCode("seller.full_name_required").WithMessage("Full name is required.")
            .MaximumLength(150).WithErrorCode("seller.full_name_too_long").WithMessage("Full name must not exceed 150 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithErrorCode("seller.phone_required").WithMessage("Phone is required.")
            .MaximumLength(20).WithErrorCode("seller.phone_too_long").WithMessage("Phone must not exceed 20 characters.");
    }
}
