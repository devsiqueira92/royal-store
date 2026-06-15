using FluentValidation;
using RoyalStore.Domain.Sellers;

namespace RoyalStore.Application.Sellers.Commands.CreateSeller;

public sealed class CreateSellerValidator : AbstractValidator<CreateSellerCommand>
{
    public CreateSellerValidator()
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
    }
}
