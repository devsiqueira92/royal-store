import { TranslateService } from '@ngx-translate/core';
import { COLOR_FAMILY_ALIASES } from '@app/shared/constants/color-family-aliases.constant';
import {
  ColorFamilyGroup,
  ColorPaletteResponse,
  ColorShadeOption,
} from '@app/shared/interfaces/color-palette.interface';

function humanize(key: string): string {
  return key
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace(/[-_]/g, ' ')
    .replace(/^./, char => char.toUpperCase());
}

export function buildColorFamilyGroups(
  response: ColorPaletteResponse,
  translate: TranslateService,
): ColorFamilyGroup[] {
  return Object.entries(response).map(([familyKey, shades]) => {
    const familyLabel = translate.instant(`enum.colorFamilies.${familyKey}`);
    const aliases = COLOR_FAMILY_ALIASES[familyKey] ?? [];
    const familyTerms = [familyKey, familyLabel, ...aliases]
      .join(' ')
      .toLowerCase();

    const items: ColorShadeOption[] = shades.flatMap(shade =>
      Object.entries(shade).map(([shadeKey, hex]) => {
        const label = humanize(shadeKey);
        return {
          value: `${familyKey}.${shadeKey}`,
          familyKey,
          shadeKey,
          hex,
          label,
          searchText: `${familyTerms} ${shadeKey} ${label}`.toLowerCase(),
        };
      }),
    );

    return { key: familyKey, label: familyLabel, items };
  });
}

export function buildColorLookup(
  groups: ColorFamilyGroup[],
): Map<string, ColorShadeOption> {
  const lookup = new Map<string, ColorShadeOption>();
  for (const group of groups) {
    for (const item of group.items) {
      lookup.set(item.value, item);
    }
  }
  return lookup;
}
