import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { AvatarModule } from 'primeng/avatar';
import { MenubarModule } from 'primeng/menubar';
import { ButtonModule } from 'primeng/button';
import { MenuItem } from 'primeng/api';
import { MenuModule } from 'primeng/menu';
import { TranslatePipe } from '@ngx-translate/core';
import { LayoutService } from '../../services/layout.service';
import { CashShiftFacade } from '@app/features/cash-shift/facade/cash-shift.facade';
import { CashShift } from '@app/features/cash-shift/interfaces/cash-shift.interface';
import { DialogManagerService } from '@app/shared/services/dialog.service';
import { CashShiftComponent } from '@app/shared/components/modal/cash-shift/cash-shift.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-header',
  imports: [MenubarModule, AvatarModule, ButtonModule, MenuModule, TranslatePipe],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent {
  private layoutService = inject(LayoutService);
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly dialogManagerService = inject(DialogManagerService);

  protected readonly isSidebarVisible = this.layoutService.sidebarVisible;
  protected readonly cashShift = toSignal(this.cashShiftFacade.current$, {
    initialValue: null as CashShift | null,
  });

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

  protected formatOpenedAt(openedAt: string): string {
    return new Date(openedAt).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' });
  }

  protected openManageCashShift(): void {
    const cashShift = this.cashShift();
    if (!cashShift) {
      return;
    }

    this.dialogManagerService.open(CashShiftComponent, {
      showHeader: false,
      width: '480px',
      height: 'auto',
      data: { cashShift },
    });
  }
}
