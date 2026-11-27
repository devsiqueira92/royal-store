import { inject, LOCALE_ID, Pipe, PipeTransform } from '@angular/core';
import { formatNumber } from '@angular/common';

/**
 * A whole-number percentage with its sign spelled out: `-5` → `−5%`, `8` → `+8%`, `0` → `0%`.
 * Meant for price-list rules, where negative is a discount and positive a surcharge.
 */
@Pipe({ name: 'signedPercent' })
export class SignedPercentPipe implements PipeTransform {
  private readonly locale = inject(LOCALE_ID);

  transform(value: number | null | undefined, digitsInfo = '1.0-2'): string {
    if (value === null || value === undefined) {
      return '';
    }

    const sign = value > 0 ? '+' : value < 0 ? '−' : '';
    return `${sign}${formatNumber(Math.abs(value), this.locale, digitsInfo)}%`;
  }
}
