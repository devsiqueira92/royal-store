using MediatR;
using RoyalStore.Application.Payments.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Payments.Commands.ActivatePayment;

public record ActivatePaymentCommand(Guid Id) : IRequest<Result<PaymentResponse>>;
