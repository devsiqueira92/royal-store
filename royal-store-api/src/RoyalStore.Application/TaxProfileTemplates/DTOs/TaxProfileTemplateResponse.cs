using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.TaxProfileTemplates.DTOs;

public record TaxProfileTemplateResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    int TargetItemType,
    string? DefaultCfop,
    string? IcmsCst,
    string? PisCofinsCst,
    string? IvaCst,
    bool IsSelectiveTax,
    bool IsActive,
    bool IsDefault,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset UpdatedAt,
    string UpdatedBy)
{
    public static TaxProfileTemplateResponse From(TaxProfileTemplate template) =>
        new(template.Id, template.TenantId, template.Name, template.Description,
            (int)template.TargetItemType, template.DefaultCfop, template.IcmsCst,
            template.PisCofinsCst, template.IvaCst, template.IsSelectiveTax,
            template.IsActive, template.IsDefault,
            template.CreatedAt, template.CreatedBy, template.UpdatedAt, template.UpdatedBy);
}
