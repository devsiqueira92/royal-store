using FluentValidation;

namespace RoyalStore.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.CategoryId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        When(x => x.Description is not null, () =>
            RuleFor(x => x.Description).MaximumLength(500));

        RuleFor(x => x.UnitOfMeasure)
            .MaximumLength(6);

        When(x => x.Ncm is not null, () =>
            RuleFor(x => x.Ncm)
                .MaximumLength(8)
                .Matches(@"^\d+$").WithMessage("NCM must contain digits only."));

        When(x => x.Cest is not null, () =>
            RuleFor(x => x.Cest)
                .MaximumLength(7)
                .Matches(@"^\d+$").WithMessage("CEST must contain digits only."));

        RuleFor(x => x.Origin).InclusiveBetween(0, 8);
    }
}
