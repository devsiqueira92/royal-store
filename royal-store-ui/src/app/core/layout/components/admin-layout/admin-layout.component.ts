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

@Component({
  selector: 'app-admin-layout',
  imports: [
    SidebarMenuComponent,
    HeaderComponent,
    RouterOutlet,
    DetailsSidebarComponent,
  ],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLayoutComponent {
  private readonly layoutService = inject(LayoutService);
  private readonly deviceService = inject(DeviceService);

  protected readonly isSidebarVisible = this.layoutService.sidebarVisible;
  protected readonly shouldPushContent = computed(() => this.canPushContent());

  private canPushContent() {
    return (
      this.layoutService.sidebarVisible() && !this.deviceService.isMobile()
    );
  }
}
