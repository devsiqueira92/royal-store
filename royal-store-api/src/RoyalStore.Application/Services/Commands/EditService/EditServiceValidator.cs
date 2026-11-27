using FluentValidation;

namespace RoyalStore.Application.Services.Commands.EditService;

public sealed class EditServiceValidator : AbstractValidator<EditServiceCommand>
{
    public EditServiceValidator()
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

        RuleFor(x => x.Nbs)
            .NotEmpty().WithMessage("NBS is required.")
            .MaximumLength(9);

        RuleFor(x => x.IssCode)
            .NotEmpty().WithMessage("ISS code is required.")
            .MaximumLength(20);

        RuleFor(x => x.TaxProfileTemplateId).NotEmpty();

        When(x => x.OverrideTaxProfile, () =>
        {
            RuleFor(x => x.PisCofinsCst).NotEmpty().MaximumLength(4)
                .WithMessage("PIS/COFINS CST is required when overriding the tax profile.");
            RuleFor(x => x.IvaCst).NotEmpty().MaximumLength(4)
                .WithMessage("IVA CST is required when overriding the tax profile.");
            RuleFor(x => x.IsSelectiveTax).NotNull()
                .WithMessage("The selective-tax flag is required when overriding the tax profile.");
        });

        RuleForEach(x => x.VariantsToAdd).Custom((v, context) =>
            ValidateVariant(v.OptionName, context));

        RuleForEach(x => x.VariantsToUpdate).Custom((v, context) =>
            ValidateVariant(v.OptionName, context));
    }

    private static void ValidateVariant(string optionName, ValidationContext<EditServiceCommand> context)
    {
        if (string.IsNullOrWhiteSpace(optionName))
            context.AddFailure("Option name is required for a service variant.");
        else if (optionName.Length > 50)
            context.AddFailure("Option name must not exceed 50 characters.");
    }
}
