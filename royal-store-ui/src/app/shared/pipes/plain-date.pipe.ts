import { formatDate } from '@angular/common';
import { inject, LOCALE_ID, Pipe, PipeTransform } from '@angular/core';

/**
 * Formats a date-only string (`yyyy-MM-dd` or `yyyy-MM`) that carries no time
 * and no zone.
 *
 * `DatePipe` cannot be used for these: it hands the string to `new Date()`,
 * which reads a date-only ISO string as UTC midnight and then renders it in the
 * local zone — so west of UTC every such date shows up one day early. Here the
 * parts are read off the string and a local date is built from them, which is
 * what the value meant in the first place.
 */
@Pipe({
  name: 'plainDate',
})
export class PlainDatePipe implements PipeTransform {
  private readonly locale = inject(LOCALE_ID);

  transform(value: string | null | undefined, format = 'MMM dd'): string {
    if (!value) {
      return '';
    }

    const [year, month, day] = value.split('-').map(Number);
    if (!year || !month) {
      return '';
    }

    return formatDate(new Date(year, month - 1, day || 1), format, this.locale);
  }
}
