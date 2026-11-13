using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.DTOs;

public record CostCenterResponse(Guid Id, string Name, string? Code, bool IsActive)
{
    public static CostCenterResponse From(CostCenter costCenter) =>
        new(costCenter.Id, costCenter.Name, costCenter.Code, costCenter.IsActive);
}
