import {
  Product,
  ProductVariant,
  StockStatus,
} from '../interfaces/product.interface';

export function getVariantStatus(variant: ProductVariant): StockStatus {
  if (variant.stockQuantity == null) {
    return 'inStock';
  }

  if (variant.stockQuantity <= 0) {
    return 'outOfStock';
  }

  if (
    variant.minStockQuantity != null &&
    variant.stockQuantity <= variant.minStockQuantity
  ) {
    return 'lowStock';
  }

  return 'inStock';
}

export function getStockStatus(product: Product): StockStatus {
  if (!product.stockTrack || product.variants.length === 0) {
    return 'inStock';
  }

  const statuses = product.variants.map(getVariantStatus);

  if (statuses.includes('outOfStock')) {
    return 'outOfStock';
  }

  if (statuses.includes('lowStock')) {
    return 'lowStock';
  }

  return 'inStock';
}
