using FluentValidation;
using RoyalStore.Domain.Sellers;

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

        RuleFor(x => x.Cpf)
            .NotEmpty().WithErrorCode("seller.cpf_required").WithMessage("CPF is required.")
            .Must(cpf => CpfValidator.IsValid(CpfValidator.Normalize(cpf ?? string.Empty)))
                .WithErrorCode("seller.invalid_cpf").WithMessage("CPF is invalid.")
            .When(x => !string.IsNullOrWhiteSpace(x.Cpf));

        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("seller.email_required").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("seller.email_invalid").WithMessage("Email is invalid.");

        When(x => !string.IsNullOrWhiteSpace(x.NewPassword), () =>
        {
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword)
                .WithErrorCode("seller.password_confirmation_mismatch")
                .WithMessage("Password confirmation does not match.");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithErrorCode("seller.current_password_required")
                .WithMessage("Current password is required to set a new password.");
        });
    }
}
