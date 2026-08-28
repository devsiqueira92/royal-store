using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Users.DTOs;
using RoyalStore.Application.Users.Services;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Identity;
using RoyalStore.Domain.Sellers;

namespace RoyalStore.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ISecurityAuditService _auditService;
    private readonly ISellerRepository _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IIdentityService identityService,
        ITenantRepository tenantRepository,
        ITenantContext tenantContext,
        ISecurityAuditService auditService,
        ISellerRepository sellerRepository,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _tenantRepository = tenantRepository;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateUserResponse>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var tenant = await _tenantRepository.FindByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
            return Error.NotFound("tenant.not_found", "Tenant not found.");

        var storeHandle = UsernameGenerator.Normalize(tenant.StoreName);
        var baseUserName = UsernameGenerator.GenerateBase(request.FirstName, request.LastName, storeHandle);

        var candidate = baseUserName;
        var suffix = 1;
        while (await _identityService.UserNameExistsInTenantAsync(candidate, tenantId, cancellationToken))
        {
            suffix++;
            var atIndex = baseUserName.IndexOf('@');
            candidate = baseUserName[..atIndex] + suffix + baseUserName[atIndex..];
        }

        var password = PasswordGenerator.Generate();

        return await _unitOfWork.ExecuteInTransactionAsync<CreateUserResponse>(async ct =>
        {
            var userResult = UserEntity.CreateSeller(candidate, tenantId);
            if (!userResult.IsSuccess)
                return userResult.Error;

            var createResult = await _identityService.CreateAsync(userResult.Value, password);
            if (!createResult.IsSuccess)
                return createResult.Error;

            var roleResult = await _identityService.AddToRoleAsync(userResult.Value, Role.Seller);
            if (!roleResult.IsSuccess)
                return roleResult.Error;

            var fullName = $"{request.FirstName} {request.LastName}";
            var sellerResult = Seller.CreateForOnboarding(tenantId, fullName, userResult.Value.Id);
            if (!sellerResult.IsSuccess)
                return sellerResult.Error;

            await _sellerRepository.AddAsync(sellerResult.Value, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            await _auditService.RecordAsync(
                "user.created",
                tenantId,
                string.Empty,
                candidate,
                ct);

            return new CreateUserResponse(candidate, password);
        }, cancellationToken);
    }
}
