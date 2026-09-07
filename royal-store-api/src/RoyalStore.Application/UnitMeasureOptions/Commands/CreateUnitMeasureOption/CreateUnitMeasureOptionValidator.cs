using FluentValidation;

namespace RoyalStore.Application.UnitMeasureOptions.Commands.CreateUnitMeasureOption;

public sealed class CreateUnitMeasureOptionValidator : AbstractValidator<CreateUnitMeasureOptionCommand>
{
    public CreateUnitMeasureOptionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
