using MediatR;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Commands.UpdateSeller;

public record UpdateSellerCommand(
    string FullName,
    string Phone,
    string Cpf,
    string Email,
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword) : IRequest<Result<SellerResponse>>;
