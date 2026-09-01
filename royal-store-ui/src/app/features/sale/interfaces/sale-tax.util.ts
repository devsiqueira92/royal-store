import { SaleLineItem } from './sale-line-item.interface';

/**
 * Effective IBPT tax rate for a line item, as a percentage.
 *
 * Federal share depends on origin (0 = national → nationalRate, otherwise the
 * imported federal rate), plus the state and municipal rates.
 */
export function lineItemTaxRate(item: SaleLineItem): number {
  const federal =
    item.origin === 0
      ? (item.nationalRate ?? 0)
      : (item.importedFederalRate ?? 0);

  return federal + (item.stateRate ?? 0) + (item.municipalRate ?? 0);
}
