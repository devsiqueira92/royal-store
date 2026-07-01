using FluentValidation;
using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Tenants.Commands.ChangePendingEmail;

public sealed record ChangePendingEmailCommand(
    string CurrentEmail,
    string Password,
    string NewEmail) : IRequest<Result>;

internal sealed class ChangePendingEmailCommandValidator : AbstractValidator<ChangePendingEmailCommand>
{
    public ChangePendingEmailCommandValidator()
    {
        RuleFor(x => x.CurrentEmail)
            .NotEmpty().WithMessage("Current email is required.")
            .EmailAddress().WithMessage("Current email must be a valid email address.")
            .MaximumLength(256).WithMessage("Current email must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("New email is required.")
            .EmailAddress().WithMessage("New email must be a valid email address.")
            .MaximumLength(256).WithMessage("New email must not exceed 256 characters.");

        RuleFor(x => x)
            .Must(x => !string.Equals(x.CurrentEmail, x.NewEmail, StringComparison.OrdinalIgnoreCase))
            .WithMessage("The new email must be different from the current one.")
            .WithName("NewEmail");
    }
}
