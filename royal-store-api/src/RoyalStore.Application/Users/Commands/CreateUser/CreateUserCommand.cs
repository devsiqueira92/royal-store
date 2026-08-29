using MediatR;
using RoyalStore.Application.Users.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Users.Commands.CreateUser;

public record CreateUserCommand(string FirstName, string LastName) : IRequest<Result<CreateUserResponse>>;
