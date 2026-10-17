using RoyalStore.Domain.Common;

namespace RoyalStore.Domain.Sales;

/// <summary>
/// Where the sale is delivered or picked up, frozen at sale time. Every field is optional;
/// <see cref="Empty"/> means no address. Not a positional record on purpose — the private constructor
/// and get-only properties make <see cref="Create"/> the only way in, so normalization can't be skipped.
/// </summary>
public sealed record SaleAddress
{
    public const int StreetMaxLength = 200;
    public const int NumberMaxLength = 20;
    public const int CityMaxLength = 100;
    public const int ZipCodeMaxLength = 20;

    public static readonly SaleAddress Empty = new(null, null, null, null);

    public string? Street { get; }

    /// <summary>Free text, so "S/N" and suffixes like "12A" are valid.</summary>
    public string? Number { get; }

    public string? City { get; }

    /// <summary>Digits only — "80010-000" is stored as "80010000". The digit count is not checked.</summary>
    public string? ZipCode { get; }

    private SaleAddress(string? street, string? number, string? city, string? zipCode)
    {
        Street = street;
        Number = number;
        City = city;
        ZipCode = zipCode;
    }

    public static Result<SaleAddress> Create(string? street, string? number, string? city, string? zipCode)
    {
        var normalizedStreet = Normalize(street);
        var normalizedNumber = Normalize(number);
        var normalizedCity = Normalize(city);
        var normalizedZipCode = NormalizeZipCode(zipCode);

        if (normalizedStreet?.Length > StreetMaxLength)
            return Error.Validation("sale.address_street_too_long",
                $"Street must be at most {StreetMaxLength} characters.");

        if (normalizedNumber?.Length > NumberMaxLength)
            return Error.Validation("sale.address_number_too_long",
                $"Number must be at most {NumberMaxLength} characters.");

        if (normalizedCity?.Length > CityMaxLength)
            return Error.Validation("sale.address_city_too_long",
                $"City must be at most {CityMaxLength} characters.");

        if (normalizedZipCode?.Length > ZipCodeMaxLength)
            return Error.Validation("sale.address_zip_code_too_long",
                $"Zip code must be at most {ZipCodeMaxLength} digits.");

        if (normalizedStreet is null && normalizedNumber is null && normalizedCity is null && normalizedZipCode is null)
            return Empty;

        return new SaleAddress(normalizedStreet, normalizedNumber, normalizedCity, normalizedZipCode);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeZipCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var digits = string.Concat(value.Where(char.IsAsciiDigit));
        return digits.Length == 0 ? null : digits;
    }
}
