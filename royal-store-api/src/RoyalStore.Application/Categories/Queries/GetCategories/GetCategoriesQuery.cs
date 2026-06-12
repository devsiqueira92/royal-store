using MediatR;
using RoyalStore.Application.Categories.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryResponse>>>;
