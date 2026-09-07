using FluentValidation;

namespace RoyalStore.Application.Products.Commands.EditProduct;

public sealed class EditProductValidator : AbstractValidator<EditProductCommand>
{
    public EditProductValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.CategoryId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BaseCostPrice).GreaterThanOrEqualTo(0);

        When(x => x.Description is not null, () =>
            RuleFor(x => x.Description).MaximumLength(500));

        RuleFor(x => x.Ncm)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("NCM is required.")
            .MaximumLength(8)
            .Matches(@"^\d+$").WithMessage("NCM must contain digits only.");

        When(x => x.Cest is not null, () =>
            RuleFor(x => x.Cest)
                .MaximumLength(7)
                .Matches(@"^\d+$").WithMessage("CEST must contain digits only."));

        RuleFor(x => x.Origin).NotEmpty().MaximumLength(2);

        RuleFor(x => x.TaxProfileTemplateId).NotEmpty();

        When(x => x.OverrideTaxProfile, () =>
        {
            RuleFor(x => x.DefaultCfop).NotEmpty().MaximumLength(4)
                .WithMessage("Default CFOP is required when overriding the tax profile.");
            RuleFor(x => x.IcmsCst).NotEmpty().MaximumLength(4)
                .WithMessage("ICMS CST is required when overriding the tax profile.");
            RuleFor(x => x.PisCofinsCst).NotEmpty().MaximumLength(4)
                .WithMessage("PIS/COFINS CST is required when overriding the tax profile.");
            RuleFor(x => x.IvaCst).NotEmpty().MaximumLength(4)
                .WithMessage("IVA CST is required when overriding the tax profile.");
            RuleFor(x => x.IsSelectiveTax).NotNull()
                .WithMessage("The selective-tax flag is required when overriding the tax profile.");
        });

        When(x => x.Measure.NetWeight.HasValue, () =>
            RuleFor(x => x.Measure.NetWeight!.Value).GreaterThanOrEqualTo(0));
        When(x => x.Measure.GrossWeight.HasValue, () =>
            RuleFor(x => x.Measure.GrossWeight!.Value).GreaterThanOrEqualTo(0));
        When(x => x.Measure.Width.HasValue, () =>
            RuleFor(x => x.Measure.Width!.Value).GreaterThanOrEqualTo(0));
        When(x => x.Measure.Height.HasValue, () =>
            RuleFor(x => x.Measure.Height!.Value).GreaterThanOrEqualTo(0));
        When(x => x.Measure.Length.HasValue, () =>
            RuleFor(x => x.Measure.Length!.Value).GreaterThanOrEqualTo(0));

        RuleForEach(x => x.VariantsToAdd).Custom((v, context) =>
        {
            var command = (EditProductCommand)context.InstanceToValidate;
            ValidateVariant(v.Size, v.Color, v.OptionName, v.Sku, v.Barcode, v.Price, v.CostPrice,
                v.StockQuantity, v.MinStockQuantity, command.StockTrack, command.BasePrice, command.BaseCostPrice, context);
        });

        RuleForEach(x => x.VariantsToUpdate).Custom((v, context) =>
        {
            var command = (EditProductCommand)context.InstanceToValidate;
            ValidateVariant(v.Size, v.Color, v.OptionName, v.Sku, v.Barcode, v.Price, v.CostPrice,
                v.StockQuantity, v.MinStockQuantity, command.StockTrack, command.BasePrice, command.BaseCostPrice, context);
        });
    }

    private static void ValidateVariant(
        string? size, string? color, string? optionName, string? sku, string? barcode,
        decimal? price, decimal? costPrice, decimal? stockQuantity, decimal? minStockQuantity,
        bool ownerStockTrack, decimal ownerBasePrice, decimal ownerBaseCostPrice,
        ValidationContext<EditProductCommand> context)
    {
        if (string.IsNullOrWhiteSpace(size) && string.IsNullOrWhiteSpace(color))
            context.AddFailure("Each variant must have at least one of Size or Color.");

        if (ownerStockTrack)
        {
            if (string.IsNullOrWhiteSpace(sku))
                context.AddFailure("SKU is required when stock is tracked.");
            if (string.IsNullOrWhiteSpace(barcode))
                context.AddFailure("Barcode is required when stock is tracked.");
            if (!stockQuantity.HasValue)
                context.AddFailure("Stock quantity is required when stock is tracked.");
            if (!minStockQuantity.HasValue)
                context.AddFailure("Minimum stock quantity is required when stock is tracked.");
        }

        if (price.HasValue)
        {
            if (price.Value < 0)
                context.AddFailure("Variant price must be zero or greater.");
            else if (price.Value < ownerBasePrice)
                context.AddFailure("Variant price must be greater than the product's base price.");
        }

        if (costPrice.HasValue)
        {
            if (costPrice.Value < 0)
                context.AddFailure("Variant cost price must be zero or greater.");
            else if (costPrice.Value < ownerBaseCostPrice)
                context.AddFailure("Variant cost price must be greater than the product's base cost price.");
        }

        if (stockQuantity is < 0)
            context.AddFailure("Stock quantity must be zero or greater.");
        if (minStockQuantity is < 0)
            context.AddFailure("Minimum stock quantity must be zero or greater.");

        if (size?.Length > 50)
            context.AddFailure("Size must not exceed 50 characters.");
        if (color?.Length > 50)
            context.AddFailure("Color must not exceed 50 characters.");
        if (optionName?.Length > 50)
            context.AddFailure("Option name must not exceed 50 characters.");
        if (sku?.Length > 50)
            context.AddFailure("SKU must not exceed 50 characters.");
        if (barcode?.Length > 14)
            context.AddFailure("Barcode must not exceed 14 characters.");
    }
}
