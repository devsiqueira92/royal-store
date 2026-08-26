using FluentValidation;

namespace RoyalStore.Application.Products.Commands.AddProductVariant;

public sealed class AddProductVariantValidator : AbstractValidator<AddProductVariantCommand>
{
    public AddProductVariantValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Size) || !string.IsNullOrWhiteSpace(x.Color))
            .WithMessage("At least one of Size or Color is required.");

        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);

        When(x => x.CostPrice.HasValue, () =>
            RuleFor(x => x.CostPrice!.Value).GreaterThanOrEqualTo(0));

        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);

        When(x => x.Size is not null, () =>
            RuleFor(x => x.Size).MaximumLength(50));

        When(x => x.Color is not null, () =>
            RuleFor(x => x.Color).MaximumLength(50));

        When(x => x.Sku is not null, () =>
            RuleFor(x => x.Sku).MaximumLength(50));

        When(x => x.Barcode is not null, () =>
            RuleFor(x => x.Barcode).MaximumLength(14));
    }
}
