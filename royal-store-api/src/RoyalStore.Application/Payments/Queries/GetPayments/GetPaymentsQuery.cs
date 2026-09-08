using MediatR;
using RoyalStore.Application.Payments.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Payments.Queries.GetPayments;

public record GetPaymentsQuery : IRequest<Result<IReadOnlyList<PaymentResponse>>>;
