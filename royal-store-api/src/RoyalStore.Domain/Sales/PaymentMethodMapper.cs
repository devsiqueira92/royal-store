using RoyalStore.Domain.Common;

namespace RoyalStore.Domain.Sales;

/// <summary>
/// Maps the point-of-sale terminal's <c>method</c> + <c>type</c> string pair onto the
/// fiscal-facing <see cref="PaymentMethod"/> enum. The raw strings are still retained on
/// <see cref="PaymentCardDetails"/> for reconciliation; this only decides the typed method.
/// </summary>
public static class PaymentMethodMapper
{
    public static Result<PaymentMethod> Map(string? method, string? type)
    {
        var normalizedMethod = method?.Trim().ToLowerInvariant();
        var normalizedType = type?.Trim().ToLowerInvariant();

        return normalizedMethod switch
        {
            "cash" or "money" or "dinheiro" => PaymentMethod.Money,
            "pix" => PaymentMethod.Pix,
            "card" or "cartao" or "cartão" => normalizedType switch
            {
                "credit" or "credito" or "crédito" => PaymentMethod.CreditCard,
                "debit" or "debito" or "débito" => PaymentMethod.DebitCard,
                _ => Error.Validation("sale.payment_method_unknown",
                    $"Card payment type '{type}' is not recognised. Expected 'credit' or 'debit'.")
            },
            _ => Error.Validation("sale.payment_method_unknown",
                $"Payment method '{method}' is not recognised.")
        };
    }
}
