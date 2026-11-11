import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TranslatePipe } from '@ngx-translate/core';
import { SaleFacade } from '../../facade/sale.facade';
import { SaleListItem } from '../../interfaces/sale-list-item.interface';

@Component({
  selector: 'app-sales-history-table',
  imports: [TableModule, TagModule, CurrencyPipe, DatePipe, TranslatePipe],
  templateUrl: './sales-history-table.component.html',
  styleUrl: './sales-history-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesHistoryTableComponent {
  private readonly saleFacade = inject(SaleFacade);

  protected readonly sales = toSignal(this.saleFacade.sales$, {
    initialValue: [] as SaleListItem[],
  });
  protected readonly loading = toSignal(this.saleFacade.isLoadingSales$, {
    initialValue: false,
  });

  protected readonly dateFormat = 'HH:mm · MMM d, y';

  protected total(sale: SaleListItem): number {
    return sale.subtotal - sale.discountAmount;
  }

  protected fulfillmentModeSeverity(
    fulfillmentMode: string,
  ): 'info' | 'warn' {
    return fulfillmentMode.toLowerCase() === 'pickup' ? 'warn' : 'info';
  }
}
