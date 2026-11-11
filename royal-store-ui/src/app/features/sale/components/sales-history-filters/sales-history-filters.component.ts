import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { SaleFacade } from '../../facade/sale.facade';
import { FulfillmentMode } from '../../interfaces/checkout.interface';

@Component({
  selector: 'app-sales-history-filters',
  imports: [
    FormsModule,
    InputTextModule,
    IconFieldModule,
    InputIconModule,
    DatePickerModule,
    SelectModule,
    TranslatePipe,
  ],
  templateUrl: './sales-history-filters.component.html',
  styleUrl: './sales-history-filters.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesHistoryFiltersComponent {
  private readonly saleFacade = inject(SaleFacade);
  private readonly translateService = inject(TranslateService);

  protected readonly sellerNameFilter = this.saleFacade.sellerNameFilter;
  protected readonly customerNameFilter = this.saleFacade.customerNameFilter;
  protected readonly soldFromFilter = this.saleFacade.soldFromFilter;
  protected readonly soldToFilter = this.saleFacade.soldToFilter;
  protected readonly fulfillmentModeFilter = this.saleFacade.fulfillmentModeFilter;

  protected readonly fulfillmentModeOptions = computed<
    { label: string; value: FulfillmentMode | null }[]
  >(() => [
    {
      label: this.translateService.instant('sale.history.fulfillmentMode.all'),
      value: null,
    },
    {
      label: this.translateService.instant(
        'sale.history.fulfillmentMode.delivery',
      ),
      value: 'delivery',
    },
    {
      label: this.translateService.instant(
        'sale.history.fulfillmentMode.pickup',
      ),
      value: 'pickup',
    },
  ]);

  protected setSellerNameFilter(sellerName: string): void {
    this.saleFacade.setSellerNameFilter(sellerName);
  }

  protected setCustomerNameFilter(customerName: string): void {
    this.saleFacade.setCustomerNameFilter(customerName);
  }

  protected setSoldFromFilter(soldFrom: Date | null): void {
    this.saleFacade.setSoldFromFilter(soldFrom);
  }

  protected setSoldToFilter(soldTo: Date | null): void {
    this.saleFacade.setSoldToFilter(soldTo);
  }

  protected setFulfillmentModeFilter(mode: FulfillmentMode | null): void {
    this.saleFacade.setFulfillmentModeFilter(mode);
  }
}
