import { ChangeDetectionStrategy, Component, computed, inject, OnInit } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { SelectModule } from 'primeng/select';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { SelectOption } from '../../interfaces/product.interface';
import { ProductFacade } from '../../facade/product.facade';
import { CategoryFacade } from '@app/features/category/facade/category.facade';
import { SupplierFacade } from '@app/features/supplier/facade/supplier.facade';

@Component({
  selector: 'app-product-filters',
  imports: [
    FormsModule,
    InputTextModule,
    IconFieldModule,
    InputIconModule,
    SelectModule,
    TranslatePipe,
  ],
  templateUrl: './product-filters.component.html',
  styleUrl: './product-filters.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductFiltersComponent implements OnInit {
  private readonly productFacade = inject(ProductFacade);
  private readonly categoryFacade = inject(CategoryFacade);
  private readonly supplierFacade = inject(SupplierFacade);
  private readonly translateService = inject(TranslateService);

  protected readonly searchTerm = this.productFacade.searchTerm;
  protected readonly categoryFilter = this.productFacade.categoryFilter;
  protected readonly supplierFilter = this.productFacade.supplierFilter;

  private readonly summary = toSignal(this.productFacade.summary$, {
    initialValue: null,
  });
  protected readonly resultsCount = computed(() => this.summary()?.result ?? 0);

  private readonly categories = toSignal(this.categoryFacade.categories$, {
    initialValue: [],
  });
  private readonly suppliers = toSignal(this.supplierFacade.suppliers$, {
    initialValue: [],
  });

  protected readonly categoryOptions = computed<SelectOption<string | null>[]>(
    () => [
      {
        label: this.translateService.instant('product.category.all'),
        value: null,
      },
      ...[...this.categories()]
        .sort((a, b) => a.name.localeCompare(b.name))
        .map(category => ({ label: category.name, value: category.id })),
    ],
  );

  protected readonly supplierOptions = computed<SelectOption<string | null>[]>(
    () => [
      {
        label: this.translateService.instant('product.supplier.all'),
        value: null,
      },
      ...[...this.suppliers()]
        .sort((a, b) => a.name.localeCompare(b.name))
        .map(supplier => ({ label: supplier.name, value: supplier.id })),
    ],
  );

  ngOnInit(): void {
    this.categoryFacade.loadCategories();
    this.supplierFacade.loadSuppliers();
  }

  protected setSearchTerm(term: string): void {
    this.productFacade.setSearchTerm(term);
  }

  protected setCategoryFilter(categoryId: string | null): void {
    this.productFacade.setCategoryFilter(categoryId);
  }

  protected setSupplierFilter(supplierId: string | null): void {
    this.productFacade.setSupplierFilter(supplierId);
  }
}
