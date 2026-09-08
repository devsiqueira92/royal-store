using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Payments;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly RoyalStoreContext _context;

    public PaymentRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<Payment> payments, CancellationToken ct = default) =>
        await _context.Payments.AddRangeAsync(payments, ct);

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Payments.ToListAsync(ct);

    public async Task<int> CountActiveAsync(CancellationToken ct = default) =>
        await _context.Payments.CountAsync(p => p.Active, ct);

    public Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        _context.Payments.Update(payment);
        return Task.CompletedTask;
    }
}
