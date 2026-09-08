using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Payments.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Payments.Queries.GetPayments;

public sealed class GetPaymentsQueryHandler
    : IRequestHandler<GetPaymentsQuery, Result<IReadOnlyList<PaymentResponse>>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentsQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<IReadOnlyList<PaymentResponse>>> Handle(
        GetPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetAllAsync(cancellationToken);
        IReadOnlyList<PaymentResponse> responses = payments.Select(PaymentResponse.From).ToList();
        return Result<IReadOnlyList<PaymentResponse>>.Success(responses);
    }
}
