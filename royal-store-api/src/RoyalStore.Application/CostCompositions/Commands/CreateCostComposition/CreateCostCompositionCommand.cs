using MediatR;
using RoyalStore.Application.CostCompositions.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.CostCompositions.Commands.CreateCostComposition;

/// <summary>
/// Creates a cost composition list on its own (064 FR-001): it needs no product and is linked to none.
/// A product save links it later by naming the identifier this returns. All lines are new — a line
/// id sent here refers to nothing.
/// </summary>
public record CreateCostCompositionCommand(string Name, IReadOnlyList<ProductCostItemInput> Items)
    : IRequest<Result<CostCompositionResponse>>
{
    // A body without "items" binds as null; an empty list is a valid list.
    public IReadOnlyList<ProductCostItemInput> Items { get; init; } = Items ?? [];
}
