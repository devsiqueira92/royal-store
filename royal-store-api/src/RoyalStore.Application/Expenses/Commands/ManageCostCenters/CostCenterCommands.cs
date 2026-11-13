using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.ManageCostCenters;

/// <summary>Adds a cost center (FR-025).</summary>
public record CreateCostCenterCommand(string Name, string? Code = null)
    : IRequest<Result<CostCenterResponse>>;

/// <summary>Renames a cost center or changes its code.</summary>
public record UpdateCostCenterCommand(Guid Id, string Name, string? Code = null)
    : IRequest<Result<CostCenterResponse>>;

/// <summary>Activates or deactivates a cost center (FR-028).</summary>
public record SetCostCenterActiveCommand(Guid Id, bool IsActive)
    : IRequest<Result<CostCenterResponse>>;

/// <summary>Deletes an unused cost center; one still referenced is refused (FR-027).</summary>
public record DeleteCostCenterCommand(Guid Id) : IRequest<Result>;
