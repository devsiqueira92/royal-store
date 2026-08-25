import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  Type,
  ViewChild,
  ViewContainerRef,
} from '@angular/core';
import { Drawer } from 'primeng/drawer';
import { DeviceService } from '../../services/device.service';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-details-sidebar',
  imports: [Drawer, CommonModule],
  templateUrl: './details-sidebar.component.html',
  styleUrl: './details-sidebar.component.scss',
})
export class DetailsSidebarComponent {
  private deviceService = inject(DeviceService);
  protected layoutService = inject(DetailsSidebarService);

  @ViewChild('dynamicContent', { read: ViewContainerRef })
  viewContainerRef!: ViewContainerRef;

  protected drawerWidth = computed(() => {
    if (this.deviceService.isMobile()) {
      return '100%';
    }

    if (this.deviceService.isTablet()) {
      return '70%';
    }

    return '600px';
  });

  constructor() {
    effect(() => {
      const content = this.layoutService.contentToRender();
      if (content && this.viewContainerRef) {
        this.loadComponent(content.component);
      } else if (
        !this.layoutService.sidebarVisible() &&
        this.viewContainerRef
      ) {
        this.viewContainerRef.clear();
      }
    });
  }

  handleVisibilityChange(isVisible: boolean) {
    if (!isVisible) {
      this.layoutService.closeDrawer();
    }
  }

  loadComponent(component: Type<any>) {
    this.viewContainerRef.clear();

    this.viewContainerRef.createComponent(component);
  }
}
