import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { SidebarMenuComponent } from '../sidebar-menu/sidebar-menu.component';
import { HeaderComponent } from '../header/header.component';
import { RouterOutlet } from '@angular/router';
import { LayoutService } from '../../services/layout.service';
import { DeviceService } from '../../services/device.service';
import { DetailsSidebarComponent } from '../details-sidebar/details-sidebar.component';
import { CashShiftFacade } from '@app/features/cash-shift/facade/cash-shift.facade';
import { SupplierFacade } from '@app/features/supplier/facade/supplier.facade';
import { AsyncPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { SkeletonModule } from 'primeng/skeleton';
import { OriginOptionFacade } from '@app/core/origin-option/facade/origin-option.facade';
import { CurrentSellerFacade } from '@app/core/current-seller/facade/current-seller.facade';
import { SessionRenewalService } from '@app/core/auth/services/session-renewal.service';

@Component({
  selector: 'app-admin-layout',
  imports: [
    SidebarMenuComponent,
    HeaderComponent,
    RouterOutlet,
    DetailsSidebarComponent,
    AsyncPipe,
    TranslatePipe,
    SkeletonModule,
  ],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLayoutComponent {
  private readonly layoutService = inject(LayoutService);
  private readonly deviceService = inject(DeviceService);
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly supplierFacade = inject(SupplierFacade);
  private readonly originOptionFacade = inject(OriginOptionFacade);
  private readonly currentSellerFacade = inject(CurrentSellerFacade);

  protected readonly isSidebarVisible = this.layoutService.sidebarVisible;
  protected readonly shouldPushContent = computed(() => this.canPushContent());
  protected readonly loaded$ = this.cashShiftFacade.currentLoaded$;
  // Injecting it starts the silent-renewal activity listeners for the app's
  // whole lifetime; nothing else here reads from it.
  private readonly sessionRenewalService = inject(SessionRenewalService);

  constructor() {
    this.cashShiftFacade.loadCurrent();
    // Loaded once here (not inside the product form) so every screen that
    // needs suppliers — today just the product form's dropdown — shares the
    // same fetch instead of re-requesting it per component instance.
    this.supplierFacade.loadSuppliers();
    this.originOptionFacade.loadOriginOptions();
    this.currentSellerFacade.loadCurrentSeller();
  }

  private canPushContent() {
    return this.layoutService.sidebarVisible() && !this.deviceService.isMobile();
  }
}
