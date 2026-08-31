import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { SaleFacade } from '../../facade/sale.facade';
import { CategoryFacade } from '@app/features/category/facade/category.facade';
import { Category } from '@app/features/category/interfaces/category.interface';
import { PosProduct } from '../../interfaces/pos-product.interface';
import { SaleLineItem } from '../../interfaces/sale-line-item.interface';
import { SalesStatsComponent } from '../../components/sales-stats/sales-stats.component';
import { CategoryFilterComponent } from '../../components/category-filter/category-filter.component';
import { ProductCardComponent } from '../../components/product-card/product-card.component';
import { CurrentSaleComponent } from '../../components/current-sale/current-sale.component';

// Mocked until auth/session and reporting are wired.
const MOCK_SELLER_NAME = 'Maya Chen';
const MOCK_SALE_REF = '#A-1042';
const MOCK_TODAY_SALES = 3248;
const MOCK_TODAY_ORDERS = 41;

@Component({
  imports: [
    TranslatePipe,
    ButtonModule,
    SalesStatsComponent,
    CategoryFilterComponent,
    ProductCardComponent,
    CurrentSaleComponent,
  ],
  templateUrl: './sales.component.html',
  styleUrl: './sales.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesComponent implements OnInit {
  private readonly saleFacade = inject(SaleFacade);
  private readonly categoryFacade = inject(CategoryFacade);

  protected readonly sellerName = MOCK_SELLER_NAME;
  protected readonly saleRef = MOCK_SALE_REF;
  protected readonly todaySales = MOCK_TODAY_SALES;
  protected readonly todayOrders = MOCK_TODAY_ORDERS;

  protected readonly products = toSignal(this.saleFacade.posProducts$, {
    initialValue: [] as PosProduct[],
  });
  protected readonly isLoading = toSignal(this.saleFacade.isLoadingProducts$, {
    initialValue: false,
  });
  protected readonly categories = toSignal(this.categoryFacade.categories$, {
    initialValue: [] as Category[],
  });

  protected readonly selectedCategoryId = signal<string | null>(null);
  protected readonly items = signal<SaleLineItem[]>([]);

  ngOnInit(): void {
    this.categoryFacade.loadCategories();
    this.saleFacade.loadPosProducts();
  }

  protected onCategoryChange(categoryId: string | null): void {
    this.selectedCategoryId.set(categoryId);
    // The category filter goes through the backend (facade → ngrx → service).
    this.saleFacade.loadPosProducts(categoryId ?? undefined);
  }

  protected addProduct(product: PosProduct): void {
    if (product.soldOut) {
      return;
    }

    this.items.update(items => {
      const existing = items.find(
        item => item.variantId === product.variantId,
      );
      if (existing) {
        return items.map(item =>
          item.variantId === product.variantId
            ? { ...item, quantity: item.quantity + 1 }
            : item,
        );
      }

      return [
        ...items,
        {
          variantId: product.variantId,
          productId: product.productId,
          name: product.productName,
          variantLabel: product.variantSize
            ? `Size ${product.variantSize.toUpperCase()}`
            : '',
          variantSize: product.variantSize,
          variantColor: product.variantColor,
          unitPrice: product.price,
          quantity: 1,
          coverPhotoUrl: product.coverImage,
        },
      ];
    });
  }

  protected changeQuantity(variantId: string, quantity: number): void {
    this.items.update(items =>
      items.map(item =>
        item.variantId === variantId ? { ...item, quantity } : item,
      ),
    );
  }

  protected removeItem(variantId: string): void {
    this.items.update(items =>
      items.filter(item => item.variantId !== variantId),
    );
  }

  protected clearSale(): void {
    this.items.set([]);
  }

  protected onCharge(): void {
    // No charge endpoint yet — completing the sale just resets the cart.
    this.items.set([]);
  }
}
