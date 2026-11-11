import { ChangeDetectionStrategy, Component, computed, inject, OnInit } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';
import { SaleFacade } from '../../facade/sale.facade';
import { SalesHistoryFiltersComponent } from '../../components/sales-history-filters/sales-history-filters.component';
import { SalesHistoryTableComponent } from '../../components/sales-history-table/sales-history-table.component';

@Component({
  imports: [TranslatePipe, SalesHistoryFiltersComponent, SalesHistoryTableComponent],
  templateUrl: './sales-history.component.html',
  styleUrl: './sales-history.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesHistoryComponent implements OnInit {
  private readonly saleFacade = inject(SaleFacade);

  private readonly sales = toSignal(this.saleFacade.sales$, { initialValue: [] });
  protected readonly totalSales = computed(() => this.sales().length);

  ngOnInit(): void {
    this.saleFacade.loadSales();
  }
}
