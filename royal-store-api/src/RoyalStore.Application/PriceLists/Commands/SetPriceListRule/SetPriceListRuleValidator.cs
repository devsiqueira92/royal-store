using FluentValidation;

namespace RoyalStore.Application.PriceLists.Commands.SetPriceListRule;

public sealed class SetPriceListRuleValidator : AbstractValidator<SetPriceListRuleCommand>
{
    public SetPriceListRuleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
