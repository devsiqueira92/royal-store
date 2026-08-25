using MediatR;
using RoyalStore.Application.Categories.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, string Name, string? Description, string? Color) : IRequest<Result<CategoryResponse>>;
