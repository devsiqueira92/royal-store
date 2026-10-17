using FluentValidation;

namespace RoyalStore.Application.Customers.Commands.AddCustomerAddress;

/// <summary>Structural only — every address field rule lives in <c>PostalAddress.Create</c>, where
/// each failure keeps its own error code.</summary>
public sealed class AddCustomerAddressValidator : AbstractValidator<AddCustomerAddressCommand>
{
    public AddCustomerAddressValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
