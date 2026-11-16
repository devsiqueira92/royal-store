using FluentValidation;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.PriceLists.Commands.CreatePriceList;

public sealed class CreatePriceListValidator : AbstractValidator<CreatePriceListCommand>
{
    public CreatePriceListValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Price list name is required.")
            .MaximumLength(PriceList.NameMaxLength);
    }
}
