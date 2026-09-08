using MediatR;
using RoyalStore.Application.Payments.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Payments.Commands.UpdatePaymentDiscount;

public record UpdatePaymentDiscountCommand(Guid Id, int Discount) : IRequest<Result<PaymentResponse>>;
