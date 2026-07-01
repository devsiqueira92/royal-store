import { effect, inject, Injectable, signal } from '@angular/core';
import { DeviceService } from './device.service';

@Injectable({
  providedIn: 'root',
})
export class LayoutService {
  sidebarVisible = signal(false);
  private deviceService = inject(DeviceService);

  constructor() {
    effect(() => {
      if (this.deviceService.isMobile()) {
        this.setSidebarState(false);
      } else {
        this.setSidebarState(true);
      }
    });
  }

  toggleSidebar() {
    this.sidebarVisible.update(val => !val);
  }

  setSidebarState(state: boolean) {
    this.sidebarVisible.set(state);
  }
}
