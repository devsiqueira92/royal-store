using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Abstractions.Data;

public sealed record CategoryWithProductCount(Category Category, int ProductCount);
