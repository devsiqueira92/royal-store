import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
} from '@angular/core';
import { SidebarMenuComponent } from '../sidebar-menu/sidebar-menu.component';
import { HeaderComponent } from '../header/header.component';
import { RouterOutlet } from '@angular/router';
import { LayoutService } from '../../services/layout.service';
import { DeviceService } from '../../services/device.service';
import { DetailsSidebarComponent } from '../details-sidebar/details-sidebar.component';
import { CashShiftFacade } from '@app/features/cash-shift/facade/cash-shift.facade';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-admin-layout',
  imports: [
    SidebarMenuComponent,
    HeaderComponent,
    RouterOutlet,
    DetailsSidebarComponent,
    AsyncPipe,
  ],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLayoutComponent {
  private readonly layoutService = inject(LayoutService);
  private readonly deviceService = inject(DeviceService);
  private readonly cashShiftFacade = inject(CashShiftFacade);

  protected readonly isSidebarVisible = this.layoutService.sidebarVisible;
  protected readonly shouldPushContent = computed(() => this.canPushContent());
  protected readonly loaded$ = this.cashShiftFacade.currentLoaded$;

  constructor() {
    this.cashShiftFacade.loadCurrent();
  }

  private canPushContent() {
    return (
      this.layoutService.sidebarVisible() && !this.deviceService.isMobile()
    );
  }
}
