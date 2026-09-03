using FluentValidation;

namespace RoyalStore.Application.Products.Commands.UpdateProductVariant;

public sealed class UpdateProductVariantValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.VariantId).NotEmpty();

        When(x => x.Price.HasValue, () =>
            RuleFor(x => x.Price!.Value).GreaterThanOrEqualTo(0));

        When(x => x.CostPrice.HasValue, () =>
            RuleFor(x => x.CostPrice!.Value).GreaterThanOrEqualTo(0));

        When(x => x.StockQuantity.HasValue, () =>
            RuleFor(x => x.StockQuantity!.Value).GreaterThanOrEqualTo(0));

        When(x => x.MinStockQuantity.HasValue, () =>
            RuleFor(x => x.MinStockQuantity!.Value).GreaterThanOrEqualTo(0));

        When(x => x.Size is not null, () =>
            RuleFor(x => x.Size).MaximumLength(50));

        When(x => x.Color is not null, () =>
            RuleFor(x => x.Color).MaximumLength(50));

        When(x => x.OptionName is not null, () =>
            RuleFor(x => x.OptionName).MaximumLength(50));

        When(x => x.Sku is not null, () =>
            RuleFor(x => x.Sku).MaximumLength(50));

        When(x => x.Barcode is not null, () =>
            RuleFor(x => x.Barcode).MaximumLength(14));
    }
}
