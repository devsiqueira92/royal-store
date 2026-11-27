import { PaymentMethod } from '@app/features/payment-method/interfaces/payment-method.interface';
import { CardPaymentType } from './payment.interface';

/** The fiscal payment code the backend's catalog seeds for cashback/loyalty redemption
 * (`FiscalPaymentType.CashbackOrLoyalty` on the API) — a payment line resolving to this code is a
 * cashback settlement, never real tender. */
export const CASHBACK_FISCAL_TYPE = 19;

/** Fiscal code of cash (`FiscalPaymentType.Cash`): the one method that can give change. */
export const CASH_FISCAL_TYPE = 1;

/** Fiscal codes (`FiscalPaymentType` on the API) of the methods charged on a card. */
const CARD_TYPE_BY_FISCAL_CODE: Readonly<Record<number, CardPaymentType>> = {
  3: 'credit',
  4: 'debit',
  5: 'store',
};

const ICON_BY_CARD_TYPE: Readonly<Record<CardPaymentType, string>> = {
  credit: 'pi pi-credit-card',
  debit: 'pi pi-building-columns',
  store: 'pi pi-shop',
};

/**
 * One row of the payment-method picker: a real, tenant-configured catalog entry (name and discount
 * as the backend defines them). `cardType` and `fiscalType` come from the catalog's fiscal
 * `paymentType` code — so a method renamed by the admin keeps behaving like the card it is — and
 * `icon` is derived client-side from both.
 */
export interface SelectablePaymentMethod {
  id: string;
  label: string;
  labelIsKey: boolean;
  /** Catalog discount as a whole percentage (0-100). */
  discount: number;
  icon: string;
  cardType: CardPaymentType | null;
  /** Paid in cash, so the cashier can type what was handed over and get the change. */
  isCash: boolean;
  /** Numeric fiscal payment code the backend expects on the charge payload. */
  fiscalType: number;
}

/**
 * Builds the picker list: every active catalog method, in the order the API returns them. The
 * cashback settlement method is left out — redeeming cashback is its own toggle, never a tender.
 */
export function toSelectableOptions(
  methods: PaymentMethod[],
): SelectablePaymentMethod[] {
  return methods
    .map(toSelectableOption)
    .filter(option => option.fiscalType !== CASHBACK_FISCAL_TYPE);
}

function toSelectableOption(method: PaymentMethod): SelectablePaymentMethod {
  const fiscalType = resolveFiscalType(method.paymentType);
  const cardType = CARD_TYPE_BY_FISCAL_CODE[fiscalType] ?? null;

  return {
    id: method.id,
    label: method.paymentMethod,
    labelIsKey: false,
    discount: method.discount,
    icon: cardType
      ? ICON_BY_CARD_TYPE[cardType]
      : resolveIcon(method.paymentMethod, fiscalType),
    cardType,
    isCash: fiscalType === CASH_FISCAL_TYPE,
    fiscalType,
  };
}

function resolveIcon(name: string, fiscalType: number): string {
  if (fiscalType === CASH_FISCAL_TYPE) {
    return 'pi pi-money-bill';
  }

  const normalized = name.toLowerCase();

  if (normalized.includes('pix')) {
    return 'pi pi-bolt';
  }

  if (normalized.includes('gift') || normalized.includes('presente')) {
    return 'pi pi-gift';
  }

  if (
    (normalized.includes('cash') && !normalized.includes('back')) ||
    normalized.includes('dinheiro')
  ) {
    return 'pi pi-money-bill';
  }

  return 'pi pi-wallet';
}

/** The catalog's fiscal payment code (e.g. "01", "19") as the number the backend expects. */
export function resolveFiscalType(paymentType: string): number {
  return Number.parseInt(paymentType, 10);
}
