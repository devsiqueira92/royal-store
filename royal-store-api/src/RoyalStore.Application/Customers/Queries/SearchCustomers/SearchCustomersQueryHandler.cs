using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Common;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Queries.SearchCustomers;

public sealed class SearchCustomersQueryHandler(ICustomerRepository customerRepository)
    : IRequestHandler<SearchCustomersQuery, Result<IReadOnlyList<CustomerSearchItemResponse>>>
{
    /// <summary>Customers returned per request: enough to pick from, small enough to stay instant.</summary>
    public const int PageSize = 5;

    /// <summary>
    /// Fewer digits than this can't be a whole phone (8 without area code) or document, and
    /// document and phone only ever match in full — so the term is then searched by name only.
    /// </summary>
    private const int MinLookupDigits = 8;

    public async Task<Result<IReadOnlyList<CustomerSearchItemResponse>>> Handle(
        SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var normalizedName = TextSearchNormalizer.Normalize(request.Search);
        var digits = new string((request.Search ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

        var customers = await customerRepository.SearchActiveAsync(
            normalizedName.Length > 0 ? normalizedName : null,
            digits.Length >= MinLookupDigits ? digits : null,
            PageSize,
            cancellationToken);

        IReadOnlyList<CustomerSearchItemResponse> items = customers
            .Where(c => c.LoyaltyTier is not null)
            .Select(c => CustomerSearchItemResponse.From(c, c.LoyaltyTier!))
            .ToList();

        return Result<IReadOnlyList<CustomerSearchItemResponse>>.Success(items);
    }
}
