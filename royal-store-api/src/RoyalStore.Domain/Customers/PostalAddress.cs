using RoyalStore.Domain.Common;

namespace RoyalStore.Domain.Customers;

/// <summary>
/// Validated, normalized fields of a Brazilian address. Every field is optional (045 clarification);
/// the zip code and state formats are checked only when given. Not a positional record on purpose —
/// the private constructor and get-only properties make <see cref="Create"/> the only way in, so the
/// rules can't be skipped.
/// </summary>
public sealed record PostalAddress
{
    public const int ZipCodeLength = 8;
    public const int StreetMaxLength = 200;
    public const int NumberMaxLength = 20;
    public const int NeighborhoodMaxLength = 100;
    public const int CityMaxLength = 100;
    public const int StateLength = 2;
    public const int ComplementMaxLength = 100;
    public const int AdditionalInfoMaxLength = 250;

    private static readonly HashSet<string> States = new(StringComparer.Ordinal)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
        "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    /// <summary>Digits only — "80010-000" is stored as "80010000".</summary>
    public string? ZipCode { get; }

    public string? Street { get; }

    /// <summary>Free text, so "S/N" and suffixes like "12A" are valid.</summary>
    public string? Number { get; }

    public string? Neighborhood { get; }
    public string? City { get; }

    /// <summary>Upper-case UF code, e.g. "PR".</summary>
    public string? State { get; }

    public string? Complement { get; }

    /// <summary>Reference point or delivery instructions.</summary>
    public string? AdditionalInfo { get; }

    public bool IsEmpty =>
        ZipCode is null && Street is null && Number is null && Neighborhood is null &&
        City is null && State is null && Complement is null && AdditionalInfo is null;

    private PostalAddress(string? zipCode, string? street, string? number, string? neighborhood,
        string? city, string? state, string? complement, string? additionalInfo)
    {
        ZipCode = zipCode;
        Street = street;
        Number = number;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        Complement = complement;
        AdditionalInfo = additionalInfo;
    }

    public static Result<PostalAddress> Create(string? zipCode, string? street, string? number,
        string? neighborhood, string? city, string? state, string? complement, string? additionalInfo)
    {
        var normalizedZipCode = NormalizeZipCode(zipCode);
        var normalizedStreet = Normalize(street);
        var normalizedNumber = Normalize(number);
        var normalizedNeighborhood = Normalize(neighborhood);
        var normalizedCity = Normalize(city);
        var normalizedState = Normalize(state)?.ToUpperInvariant();
        var normalizedComplement = Normalize(complement);
        var normalizedAdditionalInfo = Normalize(additionalInfo);

        if (normalizedZipCode is not null && normalizedZipCode.Length != ZipCodeLength)
            return Error.Validation("customer.address_zip_code_invalid",
                $"Zip code must have exactly {ZipCodeLength} digits.");

        if (normalizedState is not null && !States.Contains(normalizedState))
            return Error.Validation("customer.address_state_invalid",
                "State must be a valid Brazilian state code (e.g., SP).");

        if (normalizedStreet?.Length > StreetMaxLength)
            return Error.Validation("customer.address_street_too_long",
                $"Street must be at most {StreetMaxLength} characters.");

        if (normalizedNumber?.Length > NumberMaxLength)
            return Error.Validation("customer.address_number_too_long",
                $"Number must be at most {NumberMaxLength} characters.");

        if (normalizedNeighborhood?.Length > NeighborhoodMaxLength)
            return Error.Validation("customer.address_neighborhood_too_long",
                $"Neighborhood must be at most {NeighborhoodMaxLength} characters.");

        if (normalizedCity?.Length > CityMaxLength)
            return Error.Validation("customer.address_city_too_long",
                $"City must be at most {CityMaxLength} characters.");

        if (normalizedComplement?.Length > ComplementMaxLength)
            return Error.Validation("customer.address_complement_too_long",
                $"Complement must be at most {ComplementMaxLength} characters.");

        if (normalizedAdditionalInfo?.Length > AdditionalInfoMaxLength)
            return Error.Validation("customer.address_additional_info_too_long",
                $"Additional info must be at most {AdditionalInfoMaxLength} characters.");

        return new PostalAddress(normalizedZipCode, normalizedStreet, normalizedNumber, normalizedNeighborhood,
            normalizedCity, normalizedState, normalizedComplement, normalizedAdditionalInfo);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>Blank → null; otherwise the ASCII digits, even when none are left ("abc" → ""), so
    /// the length check rejects it instead of silently dropping a value the user did type.</summary>
    private static string? NormalizeZipCode(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : string.Concat(value.Where(char.IsAsciiDigit));
}
