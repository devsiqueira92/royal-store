using MediatR;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Queries.SearchCustomers;

/// <param name="Search">Matched against the name (part of it, ignoring accents and case), the full
/// document (CPF/CNPJ) or the full phone. Formatting is ignored for document and phone. Blank lists
/// the first customers by name.</param>
public record SearchCustomersQuery(string? Search)
    : IRequest<Result<IReadOnlyList<CustomerSearchItemResponse>>>;
