using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.AddProductPhoto;

public record AddProductPhotoCommand(Guid ProductId, string Url)
    : IRequest<Result<ProductPhotoResponse>>;
