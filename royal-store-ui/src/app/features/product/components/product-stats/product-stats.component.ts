import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';
import { ProductFacade } from '../../facade/product.facade';
import { StockStatus } from '../../interfaces/product.interface';
import { getStockStatus } from '../../utils/product-stock-status.util';

const compactCurrencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  notation: 'compact',
  maximumFractionDigits: 1,
});

@Component({
  selector: 'app-product-stats',
  imports: [TranslatePipe],
  templateUrl: './product-stats.component.html',
  styleUrl: './product-stats.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductStatsComponent {
  private readonly productFacade = inject(ProductFacade);

  protected readonly activeStatus = this.productFacade.stockStatusFilter;

  private readonly products = toSignal(this.productFacade.products$, {
    initialValue: [],
  });
  private readonly summary = toSignal(this.productFacade.summary$, {
    initialValue: null,
  });

  protected readonly stats = computed(() => {
    const products = this.products();
    const summary = this.summary();
    return {
      totalProducts: summary?.totalQuantity ?? 0,
      inventoryValueLabel: compactCurrencyFormatter.format(
        summary?.stockValue ?? 0,
      ),
      inStockCount: products.filter(p => getStockStatus(p) === 'inStock')
        .length,
      lowStockCount: products.filter(p => getStockStatus(p) === 'lowStock')
        .length,
      outOfStockCount: products.filter(p => getStockStatus(p) === 'outOfStock')
        .length,
    };
  });

  protected toggleStatus(status: StockStatus): void {
    this.productFacade.setStockStatusFilter(status);
  }
}
