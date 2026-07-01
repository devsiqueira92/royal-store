import { Injectable, inject, computed } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, startWith } from 'rxjs/operators';

// Tipagem para facilitar o uso
export type DeviceType = 'mobile' | 'tablet' | 'desktop';

@Injectable({
  providedIn: 'root',
})
export class DeviceService {
  private breakpointObserver = inject(BreakpointObserver);

  private readonly TABLET_BREAKPOINT =
    '(min-width: 768px) and (max-width: 1024px)';
  private readonly MOBILE_BREAKPOINT = '(max-width: 767.98px)';

  readonly deviceType = toSignal<DeviceType>(
    this.breakpointObserver
      .observe([this.MOBILE_BREAKPOINT, this.TABLET_BREAKPOINT])
      .pipe(
        map(result => {
          if (result.breakpoints[this.MOBILE_BREAKPOINT]) {
            return 'mobile';
          }
          if (result.breakpoints[this.TABLET_BREAKPOINT]) {
            return 'tablet';
          }
          return 'desktop'; // Se não for mobile nem tablet, assumimos desktop
        }),
        startWith('desktop' as DeviceType),
      ),
    { requireSync: true },
  );

  // 2. Signals Computados (Helpers para facilitar o uso nos templates)
  readonly isMobile = computed(() => this.deviceType() === 'mobile');
  readonly isTablet = computed(() => this.deviceType() === 'tablet');
  readonly isDesktop = computed(() => this.deviceType() === 'desktop');

  // Ex: Mobile OU Tablet (para esconder coisas em telas pequenas)
  readonly isHandheld = computed(() =>
    ['mobile', 'tablet'].includes(this.deviceType()!),
  );
}
