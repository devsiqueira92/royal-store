import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AvatarModule } from 'primeng/avatar';
import { MenubarModule } from 'primeng/menubar';
import { ButtonModule } from 'primeng/button';
import { MenuItem } from 'primeng/api';
import { MenuModule } from 'primeng/menu';
import { LayoutService } from '../../services/layout.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-header',
  imports: [MenubarModule, AvatarModule, ButtonModule, MenuModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent {
  private layoutService = inject(LayoutService);

  protected readonly isSidebarVisible = this.layoutService.sidebarVisible;

  items: MenuItem[] | undefined;

  ngOnInit() {
    this.items = [
      {
        label: 'Minha Conta',
        items: [
          {
            label: 'Perfil',
            icon: 'pi pi-user',
            command: () => {},
          },
          {
            label: 'Configurações',
            icon: 'pi pi-cog',
          },
          {
            separator: true,
          },
          {
            label: 'Logout',
            icon: 'pi pi-sign-out',
            command: () => {
              console.log('Logout');
            },
          },
        ],
      },
    ];
  }

  updateVisibility(isVisible: boolean) {
    this.layoutService.setSidebarState(isVisible);
  }
}
