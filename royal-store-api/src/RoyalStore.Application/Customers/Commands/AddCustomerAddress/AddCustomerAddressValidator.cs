using FluentValidation;

namespace RoyalStore.Application.Customers.Commands.AddCustomerAddress;

/// <summary>Structural only — every address field rule lives in <c>PostalAddress.Create</c>, where
/// each failure keeps its own error code, and the list rules in <c>Customer.ApplyAddressChanges</c>.</summary>
public sealed class AddCustomerAddressValidator : AbstractValidator<AddCustomerAddressCommand>
{
    public AddCustomerAddressValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.Addresses)
            .NotEmpty().WithMessage("At least one address is required.");

        RuleForEach(x => x.Addresses)
            .NotNull().WithMessage("Address entries must not be null.");
    }
}
