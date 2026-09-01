using FluentValidation;

namespace RoyalStore.Application.Products.Commands.UpdateProductVariant;

public sealed class UpdateProductVariantValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.VariantId).NotEmpty();

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Size) || !string.IsNullOrWhiteSpace(x.Color))
            .WithMessage("At least one of Size or Color is required.");

        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);

        When(x => x.CostPrice.HasValue, () =>
            RuleFor(x => x.CostPrice!.Value).GreaterThanOrEqualTo(0));

        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);

        When(x => x.PromotionalPrice.HasValue, () =>
        {
            RuleFor(x => x.PromotionalPrice!.Value).GreaterThanOrEqualTo(0);
            RuleFor(x => x)
                .Must(x => x.PromotionalPrice!.Value <= x.Price)
                .WithMessage("Promotional price must not exceed the regular price.");
        });

        When(x => x.NetWeight.HasValue, () =>
            RuleFor(x => x.NetWeight!.Value).GreaterThanOrEqualTo(0));
        When(x => x.GrossWeight.HasValue, () =>
            RuleFor(x => x.GrossWeight!.Value).GreaterThanOrEqualTo(0));
        When(x => x.Width.HasValue, () =>
            RuleFor(x => x.Width!.Value).GreaterThanOrEqualTo(0));
        When(x => x.Height.HasValue, () =>
            RuleFor(x => x.Height!.Value).GreaterThanOrEqualTo(0));
        When(x => x.Length.HasValue, () =>
            RuleFor(x => x.Length!.Value).GreaterThanOrEqualTo(0));

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
