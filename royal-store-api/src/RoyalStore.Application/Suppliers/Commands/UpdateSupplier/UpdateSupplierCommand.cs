using MediatR;
using RoyalStore.Application.Suppliers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Suppliers.Commands.UpdateSupplier;

public record UpdateSupplierCommand(
    Guid Id,
    string Name,
    string? Cnpj,
    string? Phone,
    string? Address,
    string? City,
    string? ZipCode) : IRequest<Result<SupplierResponse>>;
