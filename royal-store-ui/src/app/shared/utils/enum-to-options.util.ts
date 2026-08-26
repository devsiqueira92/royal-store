import { inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

export function enumToOptions<T extends Record<string, string>>(
  enumObj: T,
  translationPrefix?: string,
): { description: string; code: string }[] {
  const translateService = inject(TranslateService);
  return Object.values(enumObj).map(value => {
    const desc = translationPrefix
      ? `${translationPrefix}.${value}`
      : value.toUpperCase();

    return {
      description: translateService.instant(desc),
      code: value,
    };
  });
}
