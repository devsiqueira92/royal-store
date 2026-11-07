using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;
