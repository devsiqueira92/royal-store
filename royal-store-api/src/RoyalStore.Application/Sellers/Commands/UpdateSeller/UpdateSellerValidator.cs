using FluentValidation;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.Application.Sellers.Commands.UpdateSeller;

public sealed class UpdateSellerValidator : AbstractValidator<UpdateSellerCommand>
{
    public UpdateSellerValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithErrorCode("seller.id_required").WithMessage("Seller id is required.");

        RuleForEach(x => x.Permissions)
            .Must(name => !string.IsNullOrWhiteSpace(name) && PermissionCatalog.TryParse(name, out _))
            .WithErrorCode("permission.unknown")
            .WithMessage(name => $"'{name}' is not a known permission.");

        When(x => x.Role is not null, () =>
        {
            RuleFor(x => x.Role!)
                .Must(role => string.Equals(role, Role.Seller, StringComparison.OrdinalIgnoreCase))
                .WithErrorCode("seller.role_not_changeable")
                .WithMessage("This operation does not change roles.");
        });
    }
}
