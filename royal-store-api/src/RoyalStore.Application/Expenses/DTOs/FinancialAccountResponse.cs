using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>
/// A financial account. Carries NO balance of any kind (FR-026a) — <paramref name="OutflowInPeriod"/>
/// is computed from payments when a period is requested, and is null otherwise.
/// </summary>
public record FinancialAccountResponse(
    Guid Id,
    string Name,
    string Type,
    bool IsActive,
    decimal? OutflowInPeriod)
{
    public static FinancialAccountResponse From(FinancialAccount account, decimal? outflowInPeriod = null) =>
        new(account.Id,
            account.Name,
            ToTypeName(account.Type),
            account.IsActive,
            outflowInPeriod);

    public static string ToTypeName(FinancialAccountType type) => type switch
    {
        FinancialAccountType.Cash => "cash",
        FinancialAccountType.BankAccount => "bank-account",
        FinancialAccountType.DigitalWallet => "digital-wallet",
        _ => type.ToString().ToLowerInvariant(),
    };

    public static FinancialAccountType? ParseType(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "cash" => FinancialAccountType.Cash,
        "bank-account" => FinancialAccountType.BankAccount,
        "digital-wallet" => FinancialAccountType.DigitalWallet,
        _ => null,
    };
}
