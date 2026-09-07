using FluentValidation;

namespace RoyalStore.Application.Sales.Commands.RegisterSale;

/// <summary>
/// Structural checks only. The pipeline's <c>ValidationBehavior</c> collapses every failure into a
/// single <c>validation.failed</c> error, so the rules whose specific error code the contract
/// promises — totals reconciliation, payment-method mapping, discount mode, catalog and cash-shift
/// resolution — live in <see cref="RegisterSaleCommandHandler"/>, where each returns its own coded
/// <c>Result</c> error.
/// </summary>
public sealed class RegisterSaleValidator : AbstractValidator<RegisterSaleCommand>
{
    public RegisterSaleValidator()
    {
        RuleFor(x => x.Metadata)
            .NotNull()
            .WithMessage("Sale metadata is required.");

        RuleFor(x => x.Sale)
            .NotNull()
            .WithMessage("Sale totals are required.");

        RuleFor(x => x.Metadata.CashRegisterSessionId)
            .NotEmpty()
            .WithMessage("Cash register session is required.")
            .When(x => x.Metadata is not null);

        RuleFor(x => x.Items)
            .NotNull()
            .Must(items => items is { Count: > 0 })
            .WithMessage("A sale must have at least one item.");

        RuleFor(x => x.Payments)
            .NotNull()
            .Must(payments => payments is { Count: > 0 })
            .WithMessage("A sale must have at least one payment.");

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

            item.RuleFor(i => i.DiscountItem)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Item discount cannot be negative.");
        });

        RuleForEach(x => x.Payments).ChildRules(payment =>
        {
            payment.RuleFor(p => p.Method)
                .NotEmpty()
                .WithMessage("Payment method is required.");

            payment.RuleFor(p => p.Amount)
                .GreaterThan(0)
                .WithMessage("Payment amount must be greater than zero.");
        });
    }
}
