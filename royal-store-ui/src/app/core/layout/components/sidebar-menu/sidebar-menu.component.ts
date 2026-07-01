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
            label: 'Appointments',
            icon: 'pi pi-calendar',
            routerLink: '/appointment',
          },
          {
            label: 'Estoque',
            icon: 'pi pi-warehouse',
            routerLink: '/inventory',
          },
          {
            label: 'Pet',
            icon: 'pi pi-crown',
            routerLink: '/pet',
          },
          {
            label: 'Proprietário',
            icon: 'pi pi-user',
            routerLink: '/owner',
          },
          {
            label: 'Profissional',
            icon: 'pi pi-user',
            routerLink: '/professional',
          },
          {
            label: 'Vacinas',
            icon: 'pi pi-history',
            routerLink: '/vaccine-history',
          },
          {
            label: 'Protocolos',
            icon: 'pi pi-list-check',
            routerLink: '/protocol',
          },
        ],
      },
      {
        label: 'Financeiro',
        icon: 'pi pi-wallet',
        items: [
          {
            label: 'Despesas',
            icon: 'pi pi-receipt',
            routerLink: '/installation',
          },
          {
            label: 'Consultas',
            icon: 'pi pi-dollar',
            routerLink: '/pet',
          },
          {
            label: 'Gráfico',
            icon: 'pi pi-chart-bar',
            routerLink: '/pet',
          },
        ],
      },
      {
        label: 'Configurações',
        icon: 'pi pi-cog',
        items: [
          {
            label: 'Installation',
            icon: 'pi pi-eraser',
            routerLink: '/installation',
          },
          {
            label: 'Configuration',
            icon: 'pi pi-heart',
            routerLink: '/pet',
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
