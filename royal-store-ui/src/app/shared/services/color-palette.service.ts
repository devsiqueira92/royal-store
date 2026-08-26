import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslateService } from '@ngx-translate/core';
import { map, shareReplay } from 'rxjs';
import {
  ColorFamilyGroup,
  ColorPaletteResponse,
  ColorShadeOption,
} from '@app/shared/interfaces/color-palette.interface';
import {
  buildColorFamilyGroups,
  buildColorLookup,
} from '@app/shared/utils/color-palette.util';

@Injectable({ providedIn: 'root' })
export class ColorPaletteService {
  private readonly http = inject(HttpClient);
  private readonly translate = inject(TranslateService);

  // Mock endpoint for now; swap for `${environment.apiUrl}/api/color-palettes` once the backend exists.
  private readonly url = '/assets/mock/color-palette.json';

  private readonly families$ = this.http.get<ColorPaletteResponse>(this.url).pipe(
    map(response => buildColorFamilyGroups(response, this.translate)),
    shareReplay(1),
  );

  readonly families = toSignal(this.families$, {
    initialValue: [] as ColorFamilyGroup[],
  });

  readonly lookup = toSignal(this.families$.pipe(map(buildColorLookup)), {
    initialValue: new Map<string, ColorShadeOption>(),
  });

  getHex(value: string | null | undefined): string | undefined {
    return value ? this.lookup().get(value)?.hex : undefined;
  }

  getLabel(value: string | null | undefined): string | undefined {
    return value ? this.lookup().get(value)?.label : undefined;
  }
}
