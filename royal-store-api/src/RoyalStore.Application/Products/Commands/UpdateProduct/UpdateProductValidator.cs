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

        RuleFor(x => x.ReferenceCode)
            .NotEmpty()
            .MaximumLength(50);

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

        RuleForEach(x => x.VariantsToAdd).ChildRules(variant =>
        {
            variant.RuleFor(v => v)
                .Must(v => !string.IsNullOrWhiteSpace(v.Size) || !string.IsNullOrWhiteSpace(v.Color))
                .WithMessage("At least one of Size or Color is required.");

            variant.RuleFor(v => v.Price).GreaterThanOrEqualTo(0);

            variant.When(v => v.CostPrice.HasValue, () =>
                variant.RuleFor(v => v.CostPrice!.Value).GreaterThanOrEqualTo(0));

            variant.RuleFor(v => v.StockQuantity).GreaterThanOrEqualTo(0);

            variant.When(v => v.Size is not null, () =>
                variant.RuleFor(v => v.Size).MaximumLength(50));

            variant.When(v => v.Color is not null, () =>
                variant.RuleFor(v => v.Color).MaximumLength(50));

            variant.When(v => v.Sku is not null, () =>
                variant.RuleFor(v => v.Sku).MaximumLength(50));

            variant.When(v => v.Barcode is not null, () =>
                variant.RuleFor(v => v.Barcode).MaximumLength(14));
        });

        RuleForEach(x => x.VariantsToUpdate).ChildRules(variant =>
        {
            variant.RuleFor(v => v.VariantId).NotEmpty();

            variant.RuleFor(v => v)
                .Must(v => !string.IsNullOrWhiteSpace(v.Size) || !string.IsNullOrWhiteSpace(v.Color))
                .WithMessage("At least one of Size or Color is required.");

            variant.RuleFor(v => v.Price).GreaterThanOrEqualTo(0);

            variant.When(v => v.CostPrice.HasValue, () =>
                variant.RuleFor(v => v.CostPrice!.Value).GreaterThanOrEqualTo(0));

            variant.RuleFor(v => v.StockQuantity).GreaterThanOrEqualTo(0);

            variant.When(v => v.Size is not null, () =>
                variant.RuleFor(v => v.Size).MaximumLength(50));

            variant.When(v => v.Color is not null, () =>
                variant.RuleFor(v => v.Color).MaximumLength(50));

            variant.When(v => v.Sku is not null, () =>
                variant.RuleFor(v => v.Sku).MaximumLength(50));

            variant.When(v => v.Barcode is not null, () =>
                variant.RuleFor(v => v.Barcode).MaximumLength(14));
        });
    }
}
