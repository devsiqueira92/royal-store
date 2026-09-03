using RoyalStore.Domain.Common;

namespace RoyalStore.Domain.Catalog;

public sealed record ProductMeasure(
    decimal? NetWeight,
    decimal? GrossWeight,
    decimal? Width,
    decimal? Height,
    decimal? Length)
{
    public static readonly ProductMeasure Empty = new(null, null, null, null, null);

    public static Result<ProductMeasure> Create(
        decimal? netWeight, decimal? grossWeight, decimal? width, decimal? height, decimal? length)
    {
        if (HasNegative(netWeight, grossWeight, width, height, length))
            return Error.Validation("product.measure_negative", "Weight and dimension values must be zero or greater.");

        return new ProductMeasure(netWeight, grossWeight, width, height, length);
    }

    private static bool HasNegative(params decimal?[] values)
    {
        foreach (var value in values)
        {
            if (value is < 0)
                return true;
        }

        return false;
    }
}
