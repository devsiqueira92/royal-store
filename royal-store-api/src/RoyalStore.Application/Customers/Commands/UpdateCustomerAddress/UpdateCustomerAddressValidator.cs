using FluentValidation;

namespace RoyalStore.Application.Customers.Commands.UpdateCustomerAddress;

/// <summary>Structural only — every address field rule lives in <c>PostalAddress.Create</c>, where
/// each failure keeps its own error code.</summary>
public sealed class UpdateCustomerAddressValidator : AbstractValidator<UpdateCustomerAddressCommand>
{
    public UpdateCustomerAddressValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.AddressId).NotEmpty();
    }
}
