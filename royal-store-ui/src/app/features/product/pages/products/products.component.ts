import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { TranslatePipe } from '@ngx-translate/core';
import { ProductFacade } from '../../facade/product.facade';
import { Product } from '../../interfaces/product.interface';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { ProductFormComponent } from '../../components/product-form/product-form.component';

type StockStatus = 'inStock' | 'lowStock' | 'outOfStock';

interface SelectOption<T> {
  label: string;
  value: T;
}

const DEFAULT_CATEGORY_COLOR = 'var(--p-surface-400)';

const compactCurrencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  notation: 'compact',
  maximumFractionDigits: 1,
});

@Component({
  imports: [
    TableModule,
    ButtonModule,
    InputTextModule,
    IconFieldModule,
    InputIconModule,
    SelectModule,
    TagModule,
    FormsModule,
    TranslatePipe,
    CurrencyPipe,
  ],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductsComponent implements OnInit {
  private productFacade = inject(ProductFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly defaultCategoryColor = DEFAULT_CATEGORY_COLOR;

  protected readonly products = toSignal(this.productFacade.products$, {
    initialValue: [] as Product[],
  });
  protected readonly isLoading = toSignal(this.productFacade.isLoading$, {
    initialValue: false,
  });

  protected readonly searchTerm = signal('');
  protected readonly categoryFilter = signal<string | null>(null);
  protected readonly statusFilter = signal<StockStatus | null>(null);
  protected readonly selectedProducts = signal<Product[]>([]);

  protected readonly statusOptions: SelectOption<StockStatus | null>[] = [
    { label: 'Status', value: null },
    { label: 'In stock', value: 'inStock' },
    { label: 'Low stock', value: 'lowStock' },
    { label: 'Out of stock', value: 'outOfStock' },
  ];

  protected readonly categoryOptions = computed<SelectOption<string | null>[]>(
    () => {
      const seen = new Map<string, string>();
      for (const product of this.products()) {
        if (!seen.has(product.categoryId)) {
          seen.set(product.categoryId, product.categoryName);
        }
      }

      const options = [...seen.entries()]
        .sort(([, a], [, b]) => a.localeCompare(b))
        .map(([value, label]) => ({ label, value }));

      return [{ label: 'All categories', value: null }, ...options];
    },
  );

  protected readonly stats = computed(() => {
    const products = this.products();
    const categoryIds = new Set(products.map(product => product.categoryId));
    const inventoryValue = products.reduce(
      (total, product) => total + product.price * product.stockQuantity,
      0,
    );
    const lowStockCount = products.filter(
      product => this.getStockStatus(product) === 'lowStock',
    ).length;
    const outOfStockCount = products.filter(
      product => this.getStockStatus(product) === 'outOfStock',
    ).length;

    return {
      totalProducts: products.length,
      categoriesCount: categoryIds.size,
      inventoryValue,
      inventoryValueLabel: compactCurrencyFormatter.format(inventoryValue),
      lowStockCount,
      outOfStockCount,
    };
  });

  protected readonly filteredProducts = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const categoryId = this.categoryFilter();
    const status = this.statusFilter();

    return this.products().filter(product => {
      const matchesTerm =
        !term ||
        product.name.toLowerCase().includes(term) ||
        (product.sku?.toLowerCase().includes(term) ?? false);
      const matchesCategory = !categoryId || product.categoryId === categoryId;
      const matchesStatus = !status || this.getStockStatus(product) === status;

      return matchesTerm && matchesCategory && matchesStatus;
    });
  });

  ngOnInit(): void {
    this.productFacade.loadProducts();
  }

  protected createProduct() {
    this.detailsSidebarService.openDrawer(ProductFormComponent);
  }

  protected getStockStatus(product: Product): StockStatus {
    if (!product.isStockTracked) {
      return 'inStock';
    }

    if (product.stockQuantity <= 0) {
      return 'outOfStock';
    }

    if (
      product.minStockQuantity != null &&
      product.stockQuantity <= product.minStockQuantity
    ) {
      return 'lowStock';
    }

    return 'inStock';
  }

  protected getStatusSeverity(product: Product): 'success' | 'warn' | 'danger' {
    switch (this.getStockStatus(product)) {
      case 'outOfStock':
        return 'danger';
      case 'lowStock':
        return 'warn';
      default:
        return 'success';
    }
  }

  protected getStatusLabelKey(product: Product): string {
    return `product.status.${this.getStockStatus(product)}`;
  }
}
