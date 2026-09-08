import { PaymentMethod } from '@app/features/payment-method/interfaces/payment-method.interface';
import { CardPaymentType } from './payment.interface';

/** Split isn't a catalog row — the tenant never seeds it — so it needs a stable local identity. */
export const SPLIT_METHOD_ID = 'split';

/**
 * One row of the payment-method picker: either a real, tenant-configured catalog entry (name and
 * discount as the backend defines them) or the synthetic "split the sale" option appended after
 * them. `cardType` and `icon` are inferred client-side from the catalog name — the backend has no
 * concept of either — so a custom method the admin names "Corporate Card" behaves like a debit
 * card (brand only, no installments) unless its name also contains "credit".
 */
export interface SelectablePaymentMethod {
  id: string;
  label: string;
  /** True only for the synthetic split row — its label is an i18n key, not literal catalog text. */
  labelIsKey: boolean;
  /** Catalog discount as a whole percentage (0-100); always 0 for split. */
  discount: number;
  icon: string;
  cardType: CardPaymentType | null;
  isSplit: boolean;
}

const SPLIT_OPTION: SelectablePaymentMethod = {
  id: SPLIT_METHOD_ID,
  label: 'sale.checkout.payment.methods.split',
  labelIsKey: true,
  discount: 0,
  icon: 'pi pi-objects-column',
  cardType: null,
  isSplit: true,
};

/** Builds the full picker list: every active catalog method, in the order the API returns them,
 * followed by the split option. */
export function toSelectableOptions(
  methods: PaymentMethod[],
): SelectablePaymentMethod[] {
  return [...methods.map(toSelectableOption), SPLIT_OPTION];
}

function toSelectableOption(method: PaymentMethod): SelectablePaymentMethod {
  const cardType = resolveCardType(method.paymentMethod);

  return {
    id: method.id,
    label: method.paymentMethod,
    labelIsKey: false,
    discount: method.discount,
    icon: resolveIcon(method.paymentMethod, cardType),
    cardType,
    isSplit: false,
  };
}

function resolveCardType(name: string): CardPaymentType | null {
  const normalized = name.toLowerCase();
  if (!normalized.includes('card')) {
    return null;
  }

  return normalized.includes('credit') ? 'credit' : 'debit';
}

function resolveIcon(name: string, cardType: CardPaymentType | null): string {
  if (cardType === 'credit') {
    return 'pi pi-credit-card';
  }

  if (cardType === 'debit') {
    return 'pi pi-building-columns';
  }

  const normalized = name.toLowerCase();
  if (normalized.includes('pix')) {
    return 'pi pi-bolt';
  }

  if (normalized.includes('cash') && !normalized.includes('back')) {
    return 'pi pi-money-bill';
  }

  if (normalized.includes('cashback') || normalized.includes('loyalty')) {
    return 'pi pi-wallet';
  }

  return 'pi pi-receipt';
}
