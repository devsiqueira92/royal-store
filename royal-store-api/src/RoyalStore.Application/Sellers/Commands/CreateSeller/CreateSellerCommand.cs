using MediatR;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Commands.CreateSeller;

public record CreateSellerCommand(
    string FullName,
    string Phone,
    string Cpf,
    string? UserId) : IRequest<Result<SellerResponse>>;
