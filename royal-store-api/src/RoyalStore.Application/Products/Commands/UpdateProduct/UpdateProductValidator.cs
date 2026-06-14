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

        When(x => x.Sku is not null, () =>
            RuleFor(x => x.Sku).MaximumLength(50));

        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);

        When(x => x.CostPrice.HasValue, () =>
            RuleFor(x => x.CostPrice!.Value).GreaterThanOrEqualTo(0));

        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);

        When(x => x.MinStockQuantity.HasValue, () =>
            RuleFor(x => x.MinStockQuantity!.Value).GreaterThanOrEqualTo(0));

        RuleFor(x => x.UnitOfMeasure)
            .NotEmpty()
            .MaximumLength(6);

        When(x => x.Barcode is not null, () =>
            RuleFor(x => x.Barcode).MaximumLength(14));

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
