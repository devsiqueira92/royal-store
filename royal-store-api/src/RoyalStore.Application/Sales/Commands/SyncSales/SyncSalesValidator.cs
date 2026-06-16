using FluentValidation;

namespace RoyalStore.Application.Sales.Commands.SyncSales;

public sealed class SyncSalesValidator : AbstractValidator<SyncSalesCommand>
{
    public SyncSalesValidator()
    {
        RuleFor(x => x.Sales)
            .NotNull()
            .NotEmpty()
            .WithMessage("Sales list must not be empty.");

        RuleFor(x => x.Sales.Count)
            .LessThanOrEqualTo(500)
            .When(x => x.Sales is not null)
            .WithMessage("Batch cannot exceed 500 sales per request.");
    }
}
