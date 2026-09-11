import { PaymentMethod } from '@app/features/payment-method/interfaces/payment-method.interface';
import { CardPaymentType } from './payment.interface';

/** The fiscal payment code the backend's catalog seeds for cashback/loyalty redemption
 * (`FiscalPaymentType.CashbackOrLoyalty` on the API) — a payment line resolving to this code is a
 * cashback settlement, never real tender. */
export const CASHBACK_FISCAL_TYPE = 19;

/**
 * One row of the payment-method picker: a real, tenant-configured catalog entry (name and discount
 * as the backend defines them). `cardType`, `icon` and `fiscalType` are all derived client-side —
 * `cardType`/`icon` from the catalog name (the backend has no concept of either, so a custom method
 * the admin names "Corporate Card" behaves like a debit card unless its name also contains
 * "credit"), and `fiscalType` from the catalog's `paymentType` code.
 */
export interface SelectablePaymentMethod {
  id: string;
  label: string;
  labelIsKey: boolean;
  /** Catalog discount as a whole percentage (0-100). */
  discount: number;
  icon: string;
  cardType: CardPaymentType | null;
  /** Numeric fiscal payment code the backend expects on the charge payload. */
  fiscalType: number;
}

/** Builds the full picker list: every active catalog method, in the order the API returns them. */
export function toSelectableOptions(
  methods: PaymentMethod[],
): SelectablePaymentMethod[] {
  return methods.map(toSelectableOption);
}

function toSelectableOption(method: PaymentMethod): SelectablePaymentMethod {
  const cardType = resolveCardType(method.paymentMethod);
  const fiscalType = resolveFiscalType(method.paymentType);

  return {
    id: method.id,
    label: method.paymentMethod,
    labelIsKey: false,
    discount: method.discount,
    icon: resolveIcon(method.paymentMethod, cardType, fiscalType),
    cardType,
    fiscalType,
  };
}

function resolveCardType(name: string): CardPaymentType | null {
  const normalized = name.toLowerCase();
  if (!normalized.includes('card')) {
    return null;
  }

  return normalized.includes('credit') ? 'credit' : 'debit';
}

function resolveIcon(
  name: string,
  cardType: CardPaymentType | null,
  fiscalType: number,
): string {
  if (cardType === 'credit') {
    return 'pi pi-credit-card';
  }

  if (cardType === 'debit') {
    return 'pi pi-building-columns';
  }

  if (fiscalType === CASHBACK_FISCAL_TYPE) {
    return 'pi pi-wallet';
  }

  const normalized = name.toLowerCase();
  if (normalized.includes('pix')) {
    return 'pi pi-bolt';
  }

  if (normalized.includes('cash') && !normalized.includes('back')) {
    return 'pi pi-money-bill';
  }

  return 'pi pi-receipt';
}

/** The catalog's fiscal payment code (e.g. "01", "19") as the number the backend expects. */
export function resolveFiscalType(paymentType: string): number {
  return Number.parseInt(paymentType, 10);
}
