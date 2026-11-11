import { SaleLineItem } from './sale-line-item.interface';

/** CST origins (Tabela B) whose IBPT federal share comes from `nationalRate`; every other
 * origin (1, 2, 6, 7, 8) uses `importedFederalRate` instead. */
const NATIONAL_ORIGINS = new Set(['0', '3', '4', '5']);

/**
 * Effective IBPT tax rate for a line item, as a percentage.
 *
 * Federal share depends on origin (national origins → nationalRate, otherwise the
 * imported federal rate), plus the state and municipal rates.
 */
export function lineItemTaxRate(item: SaleLineItem): number {
  const federal = NATIONAL_ORIGINS.has(item.origin)
    ? (item.nationalRate ?? 0)
    : (item.importedFederalRate ?? 0);

  return federal + (item.stateRate ?? 0) + (item.municipalRate ?? 0);
}
