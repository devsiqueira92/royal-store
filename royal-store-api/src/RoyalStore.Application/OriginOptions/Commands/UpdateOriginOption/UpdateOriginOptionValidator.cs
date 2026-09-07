using FluentValidation;

namespace RoyalStore.Application.OriginOptions.Commands.UpdateOriginOption;

public sealed class UpdateOriginOptionValidator : AbstractValidator<UpdateOriginOptionCommand>
{
    public UpdateOriginOptionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
