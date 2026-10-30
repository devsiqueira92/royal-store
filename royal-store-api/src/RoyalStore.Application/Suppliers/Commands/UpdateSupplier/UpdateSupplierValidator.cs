using FluentValidation;

namespace RoyalStore.Application.Suppliers.Commands.UpdateSupplier;

public sealed class UpdateSupplierValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Supplier id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .MaximumLength(150).WithMessage("Supplier name must not exceed 150 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .When(x => x.Phone is not null);

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters.")
            .When(x => x.Address is not null);

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.")
            .When(x => x.City is not null);

        RuleFor(x => x.ZipCode)
            .MaximumLength(10).WithMessage("Zip code must not exceed 10 characters.")
            .When(x => x.ZipCode is not null);
    }
}
