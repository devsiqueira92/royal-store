namespace RoyalStore.Application.Loyalty.DTOs;

public record ReverseSaleCashbackResponse(Guid SaleId, decimal ClawedBack, decimal RestoredToCustomer);
