using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetSalePriceListOptions;

public sealed class GetSalePriceListOptionsQueryHandler(
    IPriceListRepository priceListRepository,
    IProductRepository productRepository)
    : IRequestHandler<GetSalePriceListOptionsQuery, Result<IReadOnlyList<SalePriceListOptionResponse>>>
{
    public async Task<Result<IReadOnlyList<SalePriceListOptionResponse>>> Handle(
        GetSalePriceListOptionsQuery query, CancellationToken cancellationToken)
    {
        // Already ordered default first, then by name.
        var lists = await priceListRepository.GetAllAsync(true, cancellationToken);

        var variantIds = query.Items.Select(i => i.VariantId).Distinct().ToList();
        var products = await productRepository.GetForPricingAsync(variantIds, cancellationToken);

        var productByVariant = products
            .SelectMany(product => product.Variants.Select(variant => (variant.Id, product)))
            .ToDictionary(pair => pair.Id, pair => pair.product);

        var missing = query.Items.FirstOrDefault(i => !productByVariant.ContainsKey(i.VariantId));
        if (missing is not null)
            return Error.Validation("sale.variant_not_found",
                $"Variant {missing.VariantId} was not found, is inactive, or does not belong to this store.");

        var options = new List<SalePriceListOptionResponse>(lists.Count);

        foreach (var list in lists)
        {
            var lines = new List<SalePriceListLineResponse>(query.Items.Count);

            foreach (var item in query.Items)
            {
                var product = productByVariant[item.VariantId];
                if (product.SalePriceOn(list) is not { } unitPrice)
                    return Error.Validation("sale.price_not_found",
                        $"Product '{product.Name}' has no price on the default list.");

                lines.Add(new SalePriceListLineResponse(item.VariantId, unitPrice, item.Quantity * unitPrice));
            }

            options.Add(new SalePriceListOptionResponse(
                list.Id, list.Name, list.IsDefault, list.DiscountPercentage, lines.Sum(l => l.LineTotal), lines));
        }

        return options;
    }
}
