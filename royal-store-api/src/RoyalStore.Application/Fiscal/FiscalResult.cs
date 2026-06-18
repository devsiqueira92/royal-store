namespace RoyalStore.Application.Fiscal;

public record FiscalResult(bool IsSuccess, string? ReceiptPrintUrl, string? ErrorMessage, bool IsDefinitiveFailure)
{
    public static FiscalResult Success(string receiptPrintUrl) =>
        new(true, receiptPrintUrl, null, false);

    public static FiscalResult Failure(string errorMessage, bool isDefinitive) =>
        new(false, null, errorMessage, isDefinitive);
}
