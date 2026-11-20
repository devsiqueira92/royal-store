using FluentValidation;

namespace RoyalStore.Application.Labels.Commands.UpdateLabel;

public sealed class UpdateLabelValidator : AbstractValidator<UpdateLabelCommand>
{
    public UpdateLabelValidator()
    {
        RuleFor(x => x).Custom((command, context) =>
        {
            LabelInputs.Validate(command.Format, command.Template, context);

            if (command.Format is not null && (command.Format.Id is null || command.Format.Id == Guid.Empty))
                context.AddFailure("Format.Id", "The label's format id is required when changing a label.");

            if (command.Template?.Id is { } templateId && templateId != command.Id)
                context.AddFailure("Template.Id", "The template id must match the label in the address.");
        });
    }
}
