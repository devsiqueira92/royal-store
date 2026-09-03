using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.DTOs;

public record ProductMeasureResponse(
    decimal? NetWeight,
    decimal? GrossWeight,
    decimal? Width,
    decimal? Height,
    decimal? Length)
{
    public static ProductMeasureResponse From(ProductMeasure measure) =>
        new(measure.NetWeight, measure.GrossWeight, measure.Width, measure.Height, measure.Length);
}
