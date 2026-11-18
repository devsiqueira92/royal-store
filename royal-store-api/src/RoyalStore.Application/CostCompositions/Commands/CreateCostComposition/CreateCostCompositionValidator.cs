using FluentValidation;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.CostCompositions.Commands.CreateCostComposition;

public sealed class CreateCostCompositionValidator : AbstractValidator<CreateCostCompositionCommand>
{
    public CreateCostCompositionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cost composition name is required.")
            .MaximumLength(CostCompositionList.NameMaxLength);

        RuleFor(x => x).Custom((command, context) =>
            CostCompositionInputs.ValidateShape(command.Items, context));
    }
}
