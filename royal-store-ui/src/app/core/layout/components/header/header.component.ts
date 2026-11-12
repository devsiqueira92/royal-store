import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { MenuModule } from 'primeng/menu';
import { PopoverModule } from 'primeng/popover';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { LayoutService } from '../../services/layout.service';
import { ThemeService } from '../../services/theme.service';
import { AuthFacade } from '@app/core/auth/facade/auth.facade';
import { CashShiftFacade } from '@app/features/cash-shift/facade/cash-shift.facade';
import { CashShift } from '@app/features/cash-shift/interfaces/cash-shift.interface';
import { DialogManagerService } from '@app/shared/services/dialog.service';
import { CashShiftComponent } from '@app/shared/components/modal/cash-shift/cash-shift.component';

/**
 * Application bar of the authenticated shell.
 *
 * Rebuilt from a `p-menubar` into a plain `<header>` with real `<button>`s:
 * the bar is not a menu, and the previous markup used anchors without `href`
 * for the bell and the avatar, which are invisible to the keyboard. Every
 * control here now has an accessible name, a focus ring and a 44px target.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-header',
  imports: [MenuModule, PopoverModule, TranslatePipe],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent {
  private readonly layoutService = inject(LayoutService);
  private readonly themeService = inject(ThemeService);
  private readonly authFacade = inject(AuthFacade);
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly dialogManagerService = inject(DialogManagerService);
  private readonly translate = inject(TranslateService);
  private readonly router = inject(Router);

  protected readonly isSidebarVisible = this.layoutService.sidebarVisible;
  protected readonly isDarkMode = this.themeService.isDarkMode;

  protected readonly cashShift = toSignal(this.cashShiftFacade.current$, {
    initialValue: null as CashShift | null,
  });

  private readonly user = toSignal(this.authFacade.user$, { initialValue: null });

  /** Re-emits on language change so the menu labels follow the active locale. */
  private readonly lang = toSignal(this.translate.onLangChange, { initialValue: null });

  protected readonly userName = computed(
    () => this.user()?.fullName?.trim() || this.user()?.email || '',
  );

  protected readonly userInitials = computed(() => {
    const name = this.userName();

    if (!name) {
      return '?';
    }

    const parts = name.split(/[\s@.]+/).filter(Boolean);
    const initials = parts.slice(0, 2).map(part => part[0]);

    return initials.join('').toUpperCase();
  });

  protected readonly items = computed<MenuItem[]>(() => {
    // Read so the labels are recomputed after a language switch.
    this.lang();

    return [
      {
        label: this.translate.instant('layout.header.account'),
        items: [
          {
            label: this.translate.instant('layout.header.profile'),
            icon: 'pi pi-user',
            command: () => void this.router.navigate(['/profile']),
          },
          { separator: true },
          {
            label: this.translate.instant('layout.header.logout'),
            icon: 'pi pi-sign-out',
            command: () => this.authFacade.logout(),
          },
        ],
      },
    ];
  });

  protected updateVisibility(isVisible: boolean): void {
    this.layoutService.setSidebarState(isVisible);
  }

  protected toggleDarkMode(isDark: boolean): void {
    this.themeService.setDarkMode(isDark);
  }

  protected formatOpenedAt(openedAt: string): string {
    return new Date(openedAt).toLocaleTimeString([], {
      hour: 'numeric',
      minute: '2-digit',
    });
  }

  protected openManageCashShift(): void {
    const cashShift = this.cashShift();

    if (!cashShift) {
      return;
    }

    this.dialogManagerService.open(CashShiftComponent, {
      showHeader: false,
      width: '480px',
      height: 'auto',
      data: { cashShift },
    });
  }
}
