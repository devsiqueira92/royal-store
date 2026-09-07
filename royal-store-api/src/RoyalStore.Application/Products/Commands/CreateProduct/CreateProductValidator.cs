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

        When(x => x.Variants is not null, () =>
            RuleForEach(x => x.Variants).Custom((v, context) =>
            {
                var command = (CreateProductCommand)context.InstanceToValidate;

                if (string.IsNullOrWhiteSpace(v.Size) && string.IsNullOrWhiteSpace(v.Color))
                    context.AddFailure("Each variant must have at least one of Size or Color.");

                if (command.StockTrack)
                {
                    if (string.IsNullOrWhiteSpace(v.Sku))
                        context.AddFailure("SKU is required when stock is tracked.");
                    if (string.IsNullOrWhiteSpace(v.Barcode))
                        context.AddFailure("Barcode is required when stock is tracked.");
                    if (!v.StockQuantity.HasValue)
                        context.AddFailure("Stock quantity is required when stock is tracked.");
                    if (!v.MinStockQuantity.HasValue)
                        context.AddFailure("Minimum stock quantity is required when stock is tracked.");
                }

                if (v.Price.HasValue)
                {
                    if (v.Price.Value < 0)
                        context.AddFailure("Variant price must be zero or greater.");
                    else if (v.Price.Value < command.BasePrice)
                        context.AddFailure("Variant price must be greater than the product's base price.");
                }

                if (v.CostPrice.HasValue)
                {
                    if (v.CostPrice.Value < 0)
                        context.AddFailure("Variant cost price must be zero or greater.");
                    else if (v.CostPrice.Value < command.BaseCostPrice)
                        context.AddFailure("Variant cost price must be greater than the product's base cost price.");
                }

                if (v.StockQuantity is < 0)
                    context.AddFailure("Stock quantity must be zero or greater.");
                if (v.MinStockQuantity is < 0)
                    context.AddFailure("Minimum stock quantity must be zero or greater.");

                if (v.Size?.Length > 50)
                    context.AddFailure("Size must not exceed 50 characters.");
                if (v.Color?.Length > 50)
                    context.AddFailure("Color must not exceed 50 characters.");
                if (v.OptionName?.Length > 50)
                    context.AddFailure("Option name must not exceed 50 characters.");
                if (v.Sku?.Length > 50)
                    context.AddFailure("SKU must not exceed 50 characters.");
                if (v.Barcode?.Length > 14)
                    context.AddFailure("Barcode must not exceed 14 characters.");
            }));
    }
}
