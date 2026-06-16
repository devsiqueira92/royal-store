using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.DTOs;

public record SalePaymentRequest(PaymentMethod Method, decimal Amount);
