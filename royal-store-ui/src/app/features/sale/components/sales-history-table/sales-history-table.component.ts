import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { TranslatePipe } from '@ngx-translate/core';
import { InfiniteScrollDirective } from '@app/shared/directives/infinite-scroll.directive';
import { SaleFacade } from '../../facade/sale.facade';
import { SaleListItem } from '../../interfaces/sale-list-item.interface';

@Component({
  selector: 'app-sales-history-table',
  imports: [
    TableModule,
    TagModule,
    ButtonModule,
    CurrencyPipe,
    DatePipe,
    TranslatePipe,
    InfiniteScrollDirective,
  ],
  templateUrl: './sales-history-table.component.html',
  styleUrl: './sales-history-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesHistoryTableComponent {
  private readonly saleFacade = inject(SaleFacade);
  private readonly router = inject(Router);

  protected readonly sales = toSignal(this.saleFacade.sales$, {
    initialValue: [] as SaleListItem[],
  });
  protected readonly loading = toSignal(this.saleFacade.isLoadingSales$, {
    initialValue: false,
  });
  protected readonly loadingMore = toSignal(this.saleFacade.isLoadingMoreSales$, {
    initialValue: false,
  });
  protected readonly hasMore = toSignal(this.saleFacade.salesHasMore$, {
    initialValue: false,
  });
  protected readonly loadMoreFailed = toSignal(this.saleFacade.salesLoadMoreFailed$, {
    initialValue: false,
  });

  /** The sentinel stays quiet while a page is loading, after a failure (retry button) and at the end. */
  protected readonly infiniteScrollDisabled = computed(
    () => !this.hasMore() || this.loading() || this.loadingMore() || this.loadMoreFailed(),
  );

  protected readonly dateFormat = 'HH:mm · MMM d, y';

  protected total(sale: SaleListItem): number {
    return sale.subtotal - sale.discountAmount;
  }

  protected fulfillmentModeSeverity(
    fulfillmentMode: string,
  ): 'info' | 'warn' {
    return fulfillmentMode.toLowerCase() === 'pickup' ? 'warn' : 'info';
  }

  protected loadMore(): void {
    this.saleFacade.loadMoreSales();
  }

  protected openDetails(sale: SaleListItem): void {
    this.router.navigate(['/sales/history', sale.id]);
  }
}
