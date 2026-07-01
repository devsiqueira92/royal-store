using FluentValidation;
using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Tenants.Commands.ResendConfirmationEmail;

public sealed record ResendConfirmationEmailCommand(string Email) : IRequest<Result>;

internal sealed class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
    public ResendConfirmationEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");
    }
}
