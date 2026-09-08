using MediatR;
using RoyalStore.Application.Payments.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Payments.Commands.DeactivatePayment;

public record DeactivatePaymentCommand(Guid Id) : IRequest<Result<PaymentResponse>>;
