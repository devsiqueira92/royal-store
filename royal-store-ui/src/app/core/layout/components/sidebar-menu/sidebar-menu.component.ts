import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { PanelMenu } from 'primeng/panelmenu';
import { MenuItem } from 'primeng/api';
import { LayoutService } from '../../services/layout.service';
import { DeviceService } from '../../services/device.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-sidebar-menu',
  imports: [DrawerModule, ButtonModule, AvatarModule, PanelMenu],
  templateUrl: './sidebar-menu.component.html',
  styleUrl: './sidebar-menu.component.scss',
})
export class SidebarMenuComponent {
  private layoutService = inject(LayoutService);
  private device = inject(DeviceService);

  // Expondo para o HTML
  protected isMobile = this.device.isMobile;

  // Lê o valor diretamente do sinal
  // No HTML, você usará visible() (com parênteses)
  protected visible = this.layoutService.sidebarVisible;

  items: MenuItem[];

  constructor() {
    this.items = [
      {
        label: 'Gestão',
        icon: 'pi pi-home',
        items: [
          {
            label: 'Dashboard',
            icon: 'pi pi-chart-bar',
            routerLink: '/dashboard',
          },
          {
            label: 'Estoque',
            icon: 'pi pi-warehouse',
            routerLink: '/inventory',
          },
          {
            label: 'PDV',
            icon: 'pi pi-dollar',
            routerLink: '/sales',
          },
        ],
      },

      {
        label: 'Configurações',
        icon: 'pi pi-cog',
        items: [
          {
            label: 'Categorias',
            icon: 'pi pi-tags',
            routerLink: '/categories',
          },
          {
            label: 'Configuration',
            icon: 'pi pi-wrench',
            routerLink: '/configuration',
          },
          {
            label: 'Vendedores',
            icon: 'pi pi-users',
            routerLink: '/salespeople',
          },
        ],
      },
    ];
  }

  // Método para fechar/atualizar via serviço
  updateVisibility(isVisible: boolean) {
    this.layoutService.setSidebarState(isVisible);
  }
}
