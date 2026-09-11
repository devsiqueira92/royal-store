using FluentValidation;

namespace RoyalStore.Application.Sales.Commands.RegisterSale;

/// <summary>
/// Structural checks only. The pipeline's <c>ValidationBehavior</c> collapses every failure into a
/// single <c>validation.failed</c> error, so the rules whose specific error code the contract
/// promises — customer/payment-method/fiscal-type resolution, fulfillment validation, totals
/// reconciliation — live in <see cref="RegisterSaleCommandHandler"/>, where each returns its own coded
/// <c>Result</c> error. There is deliberately no rule requiring <c>CashRegisterSessionId</c> — it is
/// optional and unenforced (038-register-sale-checkout FR-007).
/// </summary>
public sealed class RegisterSaleValidator : AbstractValidator<RegisterSaleCommand>
{
    public RegisterSaleValidator()
    {
        RuleFor(x => x.Checkout)
            .NotNull()
            .WithMessage("Checkout details are required.");

        RuleFor(x => x.Checkout.Fulfillment)
            .NotNull()
            .WithMessage("Fulfillment details are required.")
            .When(x => x.Checkout is not null);

        RuleFor(x => x.Checkout.Totals)
            .NotNull()
            .WithMessage("Sale totals are required.")
            .When(x => x.Checkout is not null);

        RuleFor(x => x.Items)
            .NotNull()
            .Must(items => items is { Count: > 0 })
            .WithMessage("A sale must have at least one item.");

        RuleFor(x => x.Checkout.Payments)
            .NotNull()
            .Must(payments => payments is { Count: > 0 })
            .WithMessage("A sale must have at least one payment.")
            .When(x => x.Checkout is not null);

        RuleFor(x => x.Items)
            .Must(items => items.Select(i => i.VariantId).Distinct().Count() == items.Count)
            .WithMessage("The same variant must not appear on more than one line.")
            .When(x => x.Items is { Count: > 0 });

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.VariantId)
                .NotEmpty()
                .WithMessage("Variant identifier is required.");

            item.RuleFor(i => i.ProductId)
                .NotEmpty()
                .WithMessage("Product identifier is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        });

        RuleForEach(x => x.Checkout.Payments).ChildRules(payment =>
        {
            payment.RuleFor(p => p.MethodId)
                .NotEmpty()
                .WithMessage("Payment method identifier is required.");

            payment.RuleFor(p => p.Amount)
                .GreaterThan(0)
                .WithMessage("Payment amount must be greater than zero.");
        }).When(x => x.Checkout?.Payments is { Count: > 0 });
    }
}
