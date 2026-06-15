using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Sellers;

namespace RoyalStore.Application.Sellers.Commands.CreateSeller;

public sealed class CreateSellerCommandHandler
    : IRequestHandler<CreateSellerCommand, Result<SellerResponse>>
{
    private readonly ISellerRepository _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateSellerCommandHandler(
        ISellerRepository sellerRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SellerResponse>> Handle(
        CreateSellerCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var normalizedCpf = CpfValidator.Normalize(command.Cpf);

        var cpfExists = await _sellerRepository.ExistsByCpfAsync(normalizedCpf, null, cancellationToken);
        if (cpfExists)
            return Error.Conflict("seller.cpf_already_exists", $"A seller with CPF '{normalizedCpf}' already exists.");

        var sellerResult = Seller.Create(tenantId, command.FullName, command.Phone, normalizedCpf, command.UserId);
        if (!sellerResult.IsSuccess)
            return sellerResult.Error;

        var seller = sellerResult.Value;

        await _sellerRepository.AddAsync(seller, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "seller.created",
            tenantId,
            string.Empty,
            normalizedCpf,
            cancellationToken);

        return SellerResponse.From(seller);
    }
}
