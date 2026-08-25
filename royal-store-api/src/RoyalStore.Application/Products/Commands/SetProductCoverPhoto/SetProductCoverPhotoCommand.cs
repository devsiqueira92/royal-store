using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.SetProductCoverPhoto;

public record SetProductCoverPhotoCommand(Guid ProductId, Guid PhotoId)
    : IRequest<Result<ProductResponse>>;
