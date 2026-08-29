import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TranslatePipe } from '@ngx-translate/core';
import { SellerFacade } from '../../facade/seller.facade';
import { Seller } from '../../interfaces/seller.interface';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { SellerFormComponent } from '../../components/seller-form/seller-form.component';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';

@Component({
  imports: [TableModule, ButtonModule, TranslatePipe, SwitchComponent],
  templateUrl: './sellers.component.html',
  styleUrl: './sellers.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SellersComponent implements OnInit {
  private readonly sellerFacade = inject(SellerFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly sellers = toSignal(this.sellerFacade.sellers$, {
    initialValue: [] as Seller[],
  });

  protected readonly isLoading = toSignal(this.sellerFacade.isLoading$, {
    initialValue: false,
  });

  ngOnInit(): void {
    this.sellerFacade.loadSellers();
  }

  protected editSeller(seller: Seller): void {
    this.sellerFacade.loadSeller(seller.id);
    this.detailsSidebarService.openDrawer(SellerFormComponent, {
      title: 'seller.form.edit',
      subtitle: 'seller.form.subtitle',
    });
  }

  protected getInitials(fullName: string): string {
    return fullName
      .trim()
      .split(/\s+/)
      .slice(0, 2)
      .map(part => part.charAt(0).toUpperCase())
      .join('');
  }

  protected getRoleLabelKey(seller: Seller): string | null {
    return seller.role ? `seller.form.roles.${seller.role}` : null;
  }

  protected onActiveChange(seller: Seller, active: boolean): void {
    // Only deactivation is supported by the API today; reactivation has no
    // endpoint yet, so toggling back on is a no-op and the switch reverts.
    if (!active && seller.isActive) {
      this.sellerFacade.deactivateSeller(seller.id);
    }
  }
}
