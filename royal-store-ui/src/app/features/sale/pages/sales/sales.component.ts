import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { SaleFacade } from '../../facade/sale.facade';
import { CategoryFacade } from '@app/features/category/facade/category.facade';
import { Category } from '@app/features/category/interfaces/category.interface';
import { PosProduct } from '../../interfaces/pos-product.interface';
import { SaleLineItem } from '../../interfaces/sale-line-item.interface';
import { CheckoutResult } from '../../interfaces/checkout.interface';
import { RegisterSaleRequest } from '../../interfaces/register-sale-request.interface';
import { SalesStatsComponent } from '../../components/sales-stats/sales-stats.component';
import { CategoryFilterComponent } from '../../components/category-filter/category-filter.component';
import { ProductCardComponent } from '../../components/product-card/product-card.component';
import { CurrentSaleComponent } from '../../components/current-sale/current-sale.component';
import { NotificationService } from '@app/shared/services/notification.service';
import { DialogManagerService } from '@app/shared/services/dialog.service';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { combineLatest, filter, take, tap } from 'rxjs';
import { CashShiftFacade } from '@app/features/cash-shift/facade/cash-shift.facade';
import { OpenCashShiftComponent } from '@app/features/cash-shift/components/open-cash-shift/open-cash-shift.component';

@Component({
  imports: [
    TranslatePipe,
    ButtonModule,
    SalesStatsComponent,
    CategoryFilterComponent,
    ProductCardComponent,
    CurrentSaleComponent,
    ReactiveFormsModule,
    FormsModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
  ],
  templateUrl: './sales.component.html',
  styleUrl: './sales.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesComponent implements OnInit {
  private readonly saleFacade = inject(SaleFacade);
  private readonly categoryFacade = inject(CategoryFacade);
  private readonly destroyRef = inject(DestroyRef);
  private readonly notificationService = inject(NotificationService);
  private readonly translateService = inject(TranslateService);
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly dialogManagerService = inject(DialogManagerService);

  protected readonly products = toSignal(this.saleFacade.filteredPosProducts$, {
    initialValue: [] as PosProduct[],
  });

  // still resolves while the operator has unrelated text typed into the search box.
  private readonly allProducts = toSignal(this.saleFacade.posProducts$, {
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
  protected readonly searchTerm = this.saleFacade.searchTerm;

  ngOnInit(): void {
    this.categoryFacade.loadCategories();
    this.saleFacade.loadPosProducts();
    this.loadCashShift();

    this.saleFacade.lastRegisterSaleResult$
      .pipe(
        filter(result => !!result),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.items.set([]);
        this.notificationService.success({
          detail: this.translateService.instant('sale.current.chargeSuccess'),
        });
        // Left uncleared on purpose: the checkout dialog's own receipt step reads this
        // same result reactively to know the sale it just confirmed has registered, and
        // clearing it here could race that read against this very subscription.
      });
  }

  protected onCategoryChange(categoryId: string | null): void {
    this.selectedCategoryId.set(categoryId);
    this.saleFacade.loadPosProducts(categoryId ?? undefined);
  }

  protected setSearchTerm(term: string): void {
    this.saleFacade.setSearchTerm(term);
  }

  protected addProduct(product: PosProduct): void {
    if (product.soldOut) {
      return;
    }

    this.items.update(items => {
      const existing = items.find(item => item.variantId === product.variantId);
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
          origin: product.origin,
          nationalRate: product.nationalRate,
          federalRate: product.federalRate,
          importedFederalRate: product.importedFederalRate,
          stateRate: product.stateRate,
          municipalRate: product.municipalRate,
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

  private findProduct(code: string) {
    if (!code) return;
    return this.allProducts().find(p => p.barcode === code);
  }

  private loadCashShift(): void {
    combineLatest([
      this.cashShiftFacade.currentLoaded$,
      this.cashShiftFacade.current$,
    ])
      .pipe(
        filter(([loaded]) => loaded),
        take(1),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(([, cashShift]) => {
        if (!cashShift) {
          this.openMandatoryCashShiftDialog();
        }
      });
  }

  private openMandatoryCashShiftDialog(): void {
    this.dialogManagerService.open(OpenCashShiftComponent, {
      showHeader: true,
      closable: true,
      closeOnEscape: true,
      dismissableMask: false,
      width: '440px',
      height: 'auto',
    });
  }

  protected clearSale(): void {
    this.items.set([]);
  }

  protected onCharge(checkout: CheckoutResult): void {
    const request: RegisterSaleRequest = {
      items: this.items().map(item => ({
        variantId: item.variantId,
        quantity: item.quantity,
        productId: item.productId,
      })),
      checkout: {
        customer: checkout.customer ? { id: checkout.customer.id } : null,
        useCashback: checkout.useCashback,
        fulfillment: checkout.fulfillment,
        payments: checkout.payments,
        totals: checkout.totals,
      },
    };

    this.saleFacade.registerSale(request);
  }
}
