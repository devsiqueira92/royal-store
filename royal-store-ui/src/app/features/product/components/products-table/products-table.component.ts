import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CurrencyPipe } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TranslatePipe } from '@ngx-translate/core';
import { Product } from '../../interfaces/product.interface';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { ProductFacade } from '../../facade/product.facade';
import { ProductFormComponent } from '../product-form/product-form.component';
import { getStockStatus } from '../../utils/product-stock-status.util';

const DEFAULT_CATEGORY_COLOR = 'var(--p-surface-400)';

@Component({
  selector: 'app-products-table',
  imports: [TableModule, ButtonModule, TagModule, CurrencyPipe, TranslatePipe, SwitchComponent],
  templateUrl: './products-table.component.html',
  styleUrl: './products-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductsTableComponent {
  private readonly productFacade = inject(ProductFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly products = toSignal(this.productFacade.products$, {
    initialValue: [],
  });
  protected readonly loading = toSignal(this.productFacade.isLoading$, {
    initialValue: false,
  });
  protected readonly selectedProducts = toSignal(
    this.productFacade.selectedLabelProducts$,
    { initialValue: [] as Product[] },
  );

  protected readonly defaultCategoryColor = DEFAULT_CATEGORY_COLOR;

  protected onSelectionChange(products: Product[]): void {
    this.productFacade.setSelectedLabelProducts(products);
  }

  protected editProduct(product: Product): void {
    this.productFacade.loadProduct(product.id);
    this.detailsSidebarService.openDrawer(ProductFormComponent, {
      title: 'product.form.edit',
      subtitle: 'product.form.subtitle',
    });
  }

  protected updateStatus(product: Product): void {
    if (product.isActive) {
      this.productFacade.disableProduct(product.id);
    } else {
      this.productFacade.enableProduct(product.id);
    }
  }

  protected getTotalStock(product: Product): number {
    return product.variants.reduce(
      (total, variant) => total + (variant.stockQuantity ?? 0),
      0,
    );
  }

  protected getPriceRange(
    product: Product,
  ): { min: number; max: number } | null {
    if (product.variants.length === 0) {
      return null;
    }

    const prices = product.variants.map(
      variant => variant.price ?? product.basePrice,
    );
    return { min: Math.min(...prices), max: Math.max(...prices) };
  }

  protected getVariantSummary(product: Product): string {
    if (product.variants.length === 1) {
      return product.variants[0].sku ?? '';
    }

    return product.variants.length > 0
      ? `${product.variants.length} variants`
      : '';
  }

  protected getStatusSeverity(product: Product): 'success' | 'warn' | 'danger' {
    switch (getStockStatus(product)) {
      case 'outOfStock':
        return 'danger';
      case 'lowStock':
        return 'warn';
      default:
        return 'success';
    }
  }

  protected getStatusLabelKey(product: Product): string {
    return `product.status.${getStockStatus(product)}`;
  }
}
