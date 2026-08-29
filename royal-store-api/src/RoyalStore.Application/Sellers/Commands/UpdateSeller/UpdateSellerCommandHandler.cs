using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.Application.Sellers.Commands.UpdateSeller;

public sealed class UpdateSellerCommandHandler
    : IRequestHandler<UpdateSellerCommand, Result<SellerPermissionsResponse>>
{
    private readonly ISellerRepository _sellerRepository;
    private readonly IIdentityService _identityService;
    private readonly IPermissionGrantRepository _permissionGrantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;

    public UpdateSellerCommandHandler(
        ISellerRepository sellerRepository,
        IIdentityService identityService,
        IPermissionGrantRepository permissionGrantRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext)
    {
        _sellerRepository = sellerRepository;
        _identityService = identityService;
        _permissionGrantRepository = permissionGrantRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
    }

    public async Task<Result<SellerPermissionsResponse>> Handle(
        UpdateSellerCommand command,
        CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository.GetByIdAsync(command.SellerId, cancellationToken);
        if (seller is null)
            return Error.NotFound("seller.not_found", "Seller not found.");

        if (string.IsNullOrWhiteSpace(seller.UserId))
            return Error.NotFound("seller.user_not_found", "The seller has no linked user account.");

        var user = await _identityService.FindByIdAsync(seller.UserId);
        if (user is null)
            return Error.NotFound("seller.user_not_found", "The seller has no linked user account.");

        var roles = await _identityService.GetRolesAsync(user);
        if (roles.Contains(Role.Admin))
            return Error.Validation("permission.admin_immutable",
                "An administrator's permissions cannot be changed.");

        if (command.Role is not null &&
            !string.Equals(command.Role, Role.Seller, StringComparison.OrdinalIgnoreCase))
        {
            return Error.Validation("seller.role_not_changeable", "This operation does not change roles.");
        }

        var parsed = new HashSet<Permission>();
        foreach (var name in command.Permissions)
        {
            if (!PermissionCatalog.TryParse(name, out var permission))
                return Error.Validation("permission.unknown", $"'{name}' is not a known permission.");
            parsed.Add(permission);
        }

        await _permissionGrantRepository.ReplaceForUserAsync(
            seller.UserId, _tenantContext.TenantId, parsed, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.RecordAsync(
            "seller.permissions_updated",
            _tenantContext.TenantId,
            string.Empty,
            seller.UserId,
            cancellationToken);

        var grantNames = parsed.Select(PermissionCatalog.NameOf).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var effective = EffectivePermissions.For(Role.Seller, parsed)
            .Select(PermissionCatalog.NameOf)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        return new SellerPermissionsResponse(seller.Id, seller.UserId, Role.Seller, grantNames, effective);
    }
}
