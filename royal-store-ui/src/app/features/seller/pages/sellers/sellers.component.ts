import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TableModule } from 'primeng/table';
import { TranslatePipe } from '@ngx-translate/core';
import { SellerFacade } from '../../facade/seller.facade';

@Component({
  imports: [TableModule, TranslatePipe],
  templateUrl: './sellers.component.html',
  styleUrl: './sellers.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SellersComponent implements OnInit {
  private sellerFacade = inject(SellerFacade);

  protected readonly sellers = toSignal(this.sellerFacade.sellers$, {
    initialValue: [],
  });

  protected readonly isLoading = toSignal(this.sellerFacade.isLoading$, {
    initialValue: false,
  });

  ngOnInit(): void {
    this.sellerFacade.loadSellers();
  }
}
