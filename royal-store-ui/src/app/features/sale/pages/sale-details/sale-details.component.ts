import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { map } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { SaleFacade } from '../../facade/sale.facade';
import { SaleDetails } from '../../interfaces/sale-details.interface';
import { SaleTotalsComponent } from '../../components/sale-totals/sale-totals.component';
import { SalePartiesComponent } from '../../components/sale-parties/sale-parties.component';
import { SaleItemsComponent } from '../../components/sale-items/sale-items.component';
import { SaleFiscalComponent } from '../../components/sale-fiscal/sale-fiscal.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-sale-details',
  imports: [
    RouterLink,
    TranslatePipe,
    SaleTotalsComponent,
    SalePartiesComponent,
    SaleItemsComponent,
    SaleFiscalComponent,
  ],
  templateUrl: './sale-details.component.html',
  styleUrl: './sale-details.component.scss',
})
export class SaleDetailsComponent {
  private readonly saleFacade = inject(SaleFacade);
  private readonly route = inject(ActivatedRoute);

  // paramMap instead of snapshot: the component is reused when navigating between ids.
  private readonly routeSaleId = toSignal(
    this.route.paramMap.pipe(map(params => params.get('id'))),
    { initialValue: this.route.snapshot.paramMap.get('id') },
  );

  protected readonly saleDetails = toSignal(this.saleFacade.saleDetails$, {
    initialValue: null as SaleDetails | null,
  });
  protected readonly isLoading = toSignal(
    this.saleFacade.isLoadingSaleDetails$,
    { initialValue: false },
  );

  protected readonly sale = computed(() => {
    const routeSaleId = this.routeSaleId();
    const saleDetails = this.saleDetails();
    return saleDetails?.id === routeSaleId ? saleDetails : null;
  });

  protected readonly reference = computed(
    () => this.sale()?.id.slice(-5) ?? '',
  );

  constructor() {
    effect(() => {
      const routeSaleId = this.routeSaleId();
      if (routeSaleId) {
        this.saleFacade.loadSaleDetails(routeSaleId);
      }
    });
  }
}
