import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DrawerModule } from 'primeng/drawer';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { LayoutService } from '../../services/layout.service';
import { DeviceService } from '../../services/device.service';

interface NavItem {
  readonly labelKey: string;
  readonly icon: string;
  readonly route: string;
  /** `true` keeps the item highlighted only on an exact URL match. */
  readonly exact?: boolean;
}

interface NavGroup {
  readonly id: string;
  readonly labelKey: string;
  readonly items: readonly NavItem[];
}

/**
 * Primary navigation of the authenticated shell.
 *
 * Changed from `p-panelmenu` to a flat, always-visible list: the destinations
 * are few, and an accordion hid half of them behind an extra click while giving
 * no indication of where the user currently is. Each entry is now a
 * `routerLink` with `routerLinkActive`, so the current screen is always marked.
 *
 * Labels come from the translation files (they were hard-coded Portuguese), and
 * the two entries pointing at routes that do not exist (`/dashboard`,
 * `/configuration`) were removed — they could only ever land on the 404
 * redirect.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-sidebar-menu',
  imports: [DrawerModule, RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './sidebar-menu.component.html',
  styleUrl: './sidebar-menu.component.scss',
})
export class SidebarMenuComponent {
  private readonly layoutService = inject(LayoutService);
  private readonly device = inject(DeviceService);

  protected readonly isMobile = this.device.isMobile;
  protected readonly visible = this.layoutService.sidebarVisible;

  protected readonly groups: readonly NavGroup[] = [
    {
      id: 'operation',
      labelKey: 'layout.nav.operation',
      items: [
        { labelKey: 'layout.nav.pos', icon: 'pi pi-dollar', route: '/sales', exact: true },
        { labelKey: 'layout.nav.salesHistory', icon: 'pi pi-history', route: '/sales/history' },
        { labelKey: 'layout.nav.cashShift', icon: 'pi pi-money-bill', route: '/cash-shift' },
      ],
    },
    {
      id: 'catalog',
      labelKey: 'layout.nav.catalog',
      items: [
        { labelKey: 'layout.nav.products', icon: 'pi pi-box', route: '/products' },
        { labelKey: 'layout.nav.categories', icon: 'pi pi-tags', route: '/categories' },
        { labelKey: 'layout.nav.suppliers', icon: 'pi pi-truck', route: '/suppliers' },
      ],
    },
    {
      id: 'relationship',
      labelKey: 'layout.nav.relationship',
      items: [
        { labelKey: 'layout.nav.customers', icon: 'pi pi-id-card', route: '/customers' },
        { labelKey: 'layout.nav.loyalty', icon: 'pi pi-star', route: '/loyalty' },
        { labelKey: 'layout.nav.sellers', icon: 'pi pi-users', route: '/sellers' },
      ],
    },
    {
      id: 'settings',
      labelKey: 'layout.nav.settings',
      items: [
        {
          labelKey: 'layout.nav.paymentMethods',
          icon: 'pi pi-credit-card',
          route: '/payment-methods',
        },
        {
          labelKey: 'layout.nav.taxProfiles',
          icon: 'pi pi-receipt',
          route: '/tax-profile-templates',
        },
        { labelKey: 'layout.nav.profile', icon: 'pi pi-user', route: '/profile' },
      ],
    },
  ];

  protected updateVisibility(isVisible: boolean): void {
    this.layoutService.setSidebarState(isVisible);
  }

  /** On a phone the drawer overlays the content, so a tap must close it. */
  protected onNavigate(): void {
    if (this.isMobile()) {
      this.layoutService.setSidebarState(false);
    }
  }
}
