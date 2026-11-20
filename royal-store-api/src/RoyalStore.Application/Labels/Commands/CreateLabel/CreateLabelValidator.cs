using FluentValidation;

namespace RoyalStore.Application.Labels.Commands.CreateLabel;

public sealed class CreateLabelValidator : AbstractValidator<CreateLabelCommand>
{
    private const string AssignedByStore = "Identifiers are assigned by the store; leave it empty when creating a label.";

    public CreateLabelValidator()
    {
        RuleFor(x => x).Custom((command, context) =>
        {
            LabelInputs.Validate(command.Format, command.Template, context);

            // 065 FR-006: a new label names no existing row.
            if (command.Format?.Id is not null)
                context.AddFailure("Format.Id", AssignedByStore);
            if (command.Template?.Id is not null)
                context.AddFailure("Template.Id", AssignedByStore);
            if (command.Template?.LabelFormatId is not null)
                context.AddFailure("Template.LabelFormatId", AssignedByStore);
        });
    }
}
