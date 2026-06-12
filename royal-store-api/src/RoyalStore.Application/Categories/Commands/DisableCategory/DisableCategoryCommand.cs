using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Categories.Commands.DisableCategory;

public record DisableCategoryCommand(Guid Id) : IRequest<Result>;
