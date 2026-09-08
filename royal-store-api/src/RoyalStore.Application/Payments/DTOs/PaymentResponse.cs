using RoyalStore.Domain.Payments;

namespace RoyalStore.Application.Payments.DTOs;

public record PaymentResponse(Guid Id, string PaymentMethod, string PaymentType, int Discount, bool Active)
{
    public static PaymentResponse From(Payment payment) =>
        new(payment.Id, payment.PaymentMethod, payment.PaymentType, payment.Discount, payment.Active);
}
