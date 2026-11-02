import { Component, computed, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { TranslateService } from '@ngx-translate/core';
import { LayoutService } from './core/layout/services/layout.service';
import { DeviceService } from './core/layout/services/device.service';
import { ToastModule } from 'primeng/toast';
import { OriginOptionFacade } from './core/origin-option/facade/origin-option.facade';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { CashShiftFacade } from './features/cash-shift/facade/cash-shift.facade';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastModule, ConfirmPopupModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly layoutService = inject(LayoutService);
  private readonly deviceService = inject(DeviceService);
  private translate = inject(TranslateService);

  protected readonly title = signal('royal-store-ui');

  protected readonly isSidebarVisible = this.layoutService.sidebarVisible;
  protected readonly shouldPushContent = computed(() => this.canPushContent());
  constructor() {
    this.translate.addLangs(['pt', 'en']);
  }

  private canPushContent() {
    return (
      this.layoutService.sidebarVisible() && !this.deviceService.isMobile()
    );
  }
}
