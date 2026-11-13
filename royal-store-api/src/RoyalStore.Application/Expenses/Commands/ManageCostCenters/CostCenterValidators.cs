using FluentValidation;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.ManageCostCenters;

public sealed class CreateCostCenterValidator : AbstractValidator<CreateCostCenterCommand>
{
    public CreateCostCenterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cost center name is required.")
            .MaximumLength(CostCenter.NameMaxLength);

        RuleFor(x => x.Code).MaximumLength(CostCenter.CodeMaxLength);
    }
}

public sealed class UpdateCostCenterValidator : AbstractValidator<UpdateCostCenterCommand>
{
    public UpdateCostCenterValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cost center name is required.")
            .MaximumLength(CostCenter.NameMaxLength);

        RuleFor(x => x.Code).MaximumLength(CostCenter.CodeMaxLength);
    }
}
