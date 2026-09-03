using FluentValidation;

namespace RoyalStore.Application.TaxProfileTemplates.Commands.UpdateTaxProfileTemplate;

public sealed class UpdateTaxProfileTemplateValidator : AbstractValidator<UpdateTaxProfileTemplateCommand>
{
    public UpdateTaxProfileTemplateValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        When(x => x.Description is not null, () =>
            RuleFor(x => x.Description).MaximumLength(500));

        When(x => x.DefaultCfop is not null, () => RuleFor(x => x.DefaultCfop).MaximumLength(4));
        When(x => x.IcmsCst is not null, () => RuleFor(x => x.IcmsCst).MaximumLength(4));
        When(x => x.PisCofinsCst is not null, () => RuleFor(x => x.PisCofinsCst).MaximumLength(4));
        When(x => x.IvaCst is not null, () => RuleFor(x => x.IvaCst).MaximumLength(4));
    }
}
