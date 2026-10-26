using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Queries.GetCurrentSeller;

public sealed class GetCurrentSellerQueryHandler
    : IRequestHandler<GetCurrentSellerQuery, Result<CurrentSellerResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISellerRepository _sellerRepository;
    private readonly IIdentityService _identityService;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantContext _tenantContext;

    public GetCurrentSellerQueryHandler(
        ICurrentUserService currentUserService,
        ISellerRepository sellerRepository,
        IIdentityService identityService,
        ITenantRepository tenantRepository,
        ITenantContext tenantContext)
    {
        _currentUserService = currentUserService;
        _sellerRepository = sellerRepository;
        _identityService = identityService;
        _tenantRepository = tenantRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<CurrentSellerResponse>> Handle(
        GetCurrentSellerQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Error.Unauthorized("auth.not_authenticated", "No authenticated user.");

        var seller = await _sellerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (seller is null)
            return Error.NotFound("seller.not_found", "Seller not found.");

        var user = await _identityService.FindByIdAsync(userId);
        var tenant = await _tenantRepository.FindByIdAsync(_tenantContext.TenantId, cancellationToken);

        return CurrentSellerResponse.From(seller, user?.Email, tenant?.StoreName ?? string.Empty);
    }
}
