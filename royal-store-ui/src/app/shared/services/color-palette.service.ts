import { Injectable, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { COLOR_PALETTE } from '@app/shared/constants/color-palette.constant';
import { ColorFamilyGroup, ColorShadeOption } from '@app/shared/interfaces/color-palette.interface';
import {
  buildColorFamilyGroups,
  buildColorLookup,
} from '@app/shared/utils/color-palette.util';

@Injectable({ providedIn: 'root' })
export class ColorPaletteService {
  private readonly translate = inject(TranslateService);

  readonly families: ColorFamilyGroup[] = buildColorFamilyGroups(COLOR_PALETTE, this.translate);

  private readonly lookup: Map<string, ColorShadeOption> = buildColorLookup(this.families);

  getHex(value: string | null | undefined): string | undefined {
    return value ? this.lookup.get(value)?.hex : undefined;
  }

  getLabel(value: string | null | undefined): string | undefined {
    return value ? this.lookup.get(value)?.label : undefined;
  }
}
