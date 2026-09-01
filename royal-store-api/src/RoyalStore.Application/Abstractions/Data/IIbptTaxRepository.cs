using RoyalStore.Domain.Fiscal;

namespace RoyalStore.Application.Abstractions.Data;

public interface IIbptTaxRepository
{
    Task<IbptTax?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<IbptTax>> SearchAsync(string normalizedTerm, int take, CancellationToken ct = default);
}
