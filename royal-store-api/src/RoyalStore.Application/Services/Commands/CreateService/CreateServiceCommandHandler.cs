using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Services.Commands.CreateService;

public sealed class CreateServiceCommandHandler
    : IRequestHandler<CreateServiceCommand, Result<ProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITaxProfileTemplateRepository _taxProfileTemplateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateServiceCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ITaxProfileTemplateRepository taxProfileTemplateRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _taxProfileTemplateRepository = taxProfileTemplateRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ProductResponse>> Handle(
        CreateServiceCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null || !category.IsActive)
            return Error.Validation("product.category_invalid", "Category not found or inactive.");

        var taxProfileTemplate = await _taxProfileTemplateRepository.GetByIdAsync(command.TaxProfileTemplateId, cancellationToken);
        if (taxProfileTemplate is null || !taxProfileTemplate.IsActive)
            return Error.NotFound("product.tax_profile_template_not_found", "Tax profile template not found or inactive.");

        var serviceResult = Product.CreateService(
            tenantId, command.CategoryId, command.Name, command.Sku,
            command.BasePrice, command.BaseCostPrice, command.Description, command.UnitOfMeasure,
            command.Nbs, command.IssCode,
            taxProfileTemplate, command.OverrideTaxProfile,
            command.PisCofinsCst, command.IvaCst, command.IsSelectiveTax);

        if (!serviceResult.IsSuccess)
            return serviceResult.Error;

        var service = serviceResult.Value;

        foreach (var v in command.Variants ?? [])
        {
            var variantResult = service.AddVariant(tenantId, null, null, v.OptionName, null, null,
                v.Price, v.CostPrice, null, null);
            if (!variantResult.IsSuccess)
                return variantResult.Error;
        }

        await _productRepository.AddAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "service.created",
            tenantId,
            $"Service '{service.Name}' created. Id: {service.Id}",
            userId,
            cancellationToken);

        var saved = await _productRepository.GetByIdAsync(service.Id, cancellationToken);
        return ProductResponse.From(saved!);
    }
}
