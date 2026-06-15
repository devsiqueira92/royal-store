using MediatR;
using RoyalStore.Application.Users.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Users.Commands.CreateSeller;

public record CreateSellerCommand(string FirstName, string LastName) : IRequest<Result<CreateSellerResponse>>;
