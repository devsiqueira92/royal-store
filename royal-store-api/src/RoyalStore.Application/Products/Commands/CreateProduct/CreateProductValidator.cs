using FluentValidation;

namespace RoyalStore.Application.Products.Commands.CreateProduct;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
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

        When(x => x.Variants is not null, () =>
            RuleForEach(x => x.Variants).ChildRules(v =>
            {
                v.RuleFor(x => x)
                    .Must(x => !string.IsNullOrWhiteSpace(x.Size) || !string.IsNullOrWhiteSpace(x.Color))
                    .WithMessage("Each variant must have at least one of Size or Color.");

                v.RuleFor(x => x.Price).GreaterThanOrEqualTo(0);

                v.When(x => x.CostPrice.HasValue, () =>
                    v.RuleFor(x => x.CostPrice!.Value).GreaterThanOrEqualTo(0));

                v.RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);

                v.When(x => x.Size is not null, () =>
                    v.RuleFor(x => x.Size).MaximumLength(50));

                v.When(x => x.Color is not null, () =>
                    v.RuleFor(x => x.Color).MaximumLength(50));

                v.When(x => x.Sku is not null, () =>
                    v.RuleFor(x => x.Sku).MaximumLength(50));

                v.When(x => x.Barcode is not null, () =>
                    v.RuleFor(x => x.Barcode).MaximumLength(14));
            }));
    }
}
