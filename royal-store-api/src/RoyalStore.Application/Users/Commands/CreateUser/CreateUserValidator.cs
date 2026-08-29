using FluentValidation;

namespace RoyalStore.Application.Users.Commands.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithErrorCode("seller.first_name_required").WithMessage("First name is required.")
            .MaximumLength(50).WithErrorCode("seller.first_name_too_long").WithMessage("First name must not exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithErrorCode("seller.last_name_required").WithMessage("Last name is required.")
            .MaximumLength(50).WithErrorCode("seller.last_name_too_long").WithMessage("Last name must not exceed 50 characters.");
    }
}
