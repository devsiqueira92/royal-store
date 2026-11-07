import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TranslatePipe } from '@ngx-translate/core';
import { ProductFacade } from '../../facade/product.facade';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { ProductFormComponent } from '../../components/product-form/product-form.component';
import { ProductStatsComponent } from '../../components/product-stats/product-stats.component';
import { ProductFiltersComponent } from '../../components/product-filters/product-filters.component';
import { ProductsTableComponent } from '../../components/products-table/products-table.component';

@Component({
  imports: [
    RouterLink,
    ButtonModule,
    TranslatePipe,
    ProductStatsComponent,
    ProductFiltersComponent,
    ProductsTableComponent,
  ],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductsComponent implements OnInit {
  private productFacade = inject(ProductFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  private readonly products = toSignal(this.productFacade.products$, {
    initialValue: [],
  });
  protected readonly selectedProducts = toSignal(
    this.productFacade.selectedLabelProducts$,
    { initialValue: [] },
  );

  protected readonly totalProducts = computed(() => this.products().length);
  protected readonly categoriesCount = computed(
    () => new Set(this.products().map(product => product.categoryId)).size,
  );

  ngOnInit(): void {
    this.productFacade.loadProducts();
    this.productFacade.loadProductsSummary();
  }

  protected createProduct() {
    this.productFacade.clearSelectedProduct();
    this.detailsSidebarService.openDrawer(ProductFormComponent, {
      title: 'product.form.title',
      subtitle: 'product.form.subtitle',
    });
  }
}
