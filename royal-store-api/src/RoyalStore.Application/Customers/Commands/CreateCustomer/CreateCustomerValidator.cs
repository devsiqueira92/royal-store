using FluentValidation;

namespace RoyalStore.Application.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(150).WithMessage("Customer name must not exceed 150 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .When(x => x.Phone is not null);

        // Structural only: field rules live in PostalAddress.Create, list rules in the aggregate.
        RuleForEach(x => x.Addresses)
            .NotNull().WithMessage("Address entries must not be null.")
            .When(x => x.Addresses is not null);
    }
}
