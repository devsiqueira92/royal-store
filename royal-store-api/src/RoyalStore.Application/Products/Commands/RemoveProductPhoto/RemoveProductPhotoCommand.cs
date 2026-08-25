using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.RemoveProductPhoto;

public record RemoveProductPhotoCommand(Guid ProductId, Guid PhotoId) : IRequest<Result>;
