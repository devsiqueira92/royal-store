using FluentValidation;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.ReverseExpensePayment;

public sealed class ReverseExpensePaymentValidator : AbstractValidator<ReverseExpensePaymentCommand>
{
    public ReverseExpensePaymentValidator()
    {
        RuleFor(x => x.ExpenseId).NotEmpty();
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(ExpensePayment.ReversalReasonMaxLength);
    }
}
