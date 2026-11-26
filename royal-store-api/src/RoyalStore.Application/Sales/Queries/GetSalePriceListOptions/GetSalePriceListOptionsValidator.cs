using FluentValidation;

namespace RoyalStore.Application.Sales.Queries.GetSalePriceListOptions;

public sealed class GetSalePriceListOptionsValidator : AbstractValidator<GetSalePriceListOptionsQuery>
{
    public GetSalePriceListOptionsValidator()
    {
        RuleFor(x => x.Items)
            .NotNull()
            .Must(items => items is { Count: > 0 })
            .WithMessage("At least one item is required.");

        RuleFor(x => x.Items)
            .Must(items => items.Select(i => i.VariantId).Distinct().Count() == items.Count)
            .WithMessage("The same variant must not appear on more than one line.")
            .When(x => x.Items is { Count: > 0 });

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.VariantId)
                .NotEmpty()
                .WithMessage("Variant identifier is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        });
    }
}
