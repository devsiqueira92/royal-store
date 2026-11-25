using FluentValidation;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.PriceLists.Commands.UpdatePriceList;

public sealed class UpdatePriceListValidator : AbstractValidator<UpdatePriceListCommand>
{
    public UpdatePriceListValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Price list name is required.")
            .MaximumLength(PriceList.NameMaxLength);
    }
}
