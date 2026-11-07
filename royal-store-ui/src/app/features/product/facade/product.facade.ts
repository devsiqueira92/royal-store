import { computed, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { combineLatest, debounceTime, map, skip } from 'rxjs';
import { CreateProductRequest } from '../interfaces/create-product-request.interface';
import { UpdateProductRequest } from '../interfaces/update-product-request.interface';
import { GetProductsParams } from '../interfaces/get-products-params.interface';
import { GetProductsSummaryParams } from '../interfaces/products-summary.interface';
import { Product, StockStatus } from '../interfaces/product.interface';
import { ProductActions } from '../store/product.actions';
import {
  selectAllProducts,
  selectSelectedProduct,
  selectSelectedLabelProducts,
  selectSummary,
  selectIsLoading,
  selectError,
  productVariants,
} from '../store/product.selectors';

@Injectable({
  providedIn: 'root',
})
export class ProductFacade {
  private store = inject(Store);

  products$ = this.store.select(selectAllProducts);
  selectedProduct$ = this.store.select(selectSelectedProduct);
  selectedLabelProducts$ = this.store.select(selectSelectedLabelProducts);
  summary$ = this.store.select(selectSummary);
  productVariants$ = this.store.select(productVariants);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  // Filter state driving the server-side list: each setter below mutates one
  // of these signals, which are combined into a single GetProductsQuery and
  // re-dispatched automatically (see constructor) — callers never build
  // GetProductsParams or call loadProducts themselves for filtering.
  private readonly _searchTerm = signal('');
  private readonly _categoryFilter = signal<string | null>(null);
  private readonly _supplierFilter = signal<string | null>(null);
  private readonly _stockStatusFilter = signal<StockStatus | null>(null);

  readonly searchTerm = this._searchTerm.asReadonly();
  readonly categoryFilter = this._categoryFilter.asReadonly();
  readonly supplierFilter = this._supplierFilter.asReadonly();
  readonly stockStatusFilter = this._stockStatusFilter.asReadonly();

  private readonly structuredFilters = computed(() => ({
    categoryId: this._categoryFilter() ?? undefined,
    supplierId: this._supplierFilter() ?? undefined,
    stockStatus: this._stockStatusFilter() ?? undefined,
  }));

  // GET /api/products/summary only accepts categoryId/supplierId (no
  // stockStatus or search — see contracts/products-summary-api.md), so it
  // reacts to its own, narrower slice of the filter state.
  private readonly summaryFilters = computed<GetProductsSummaryParams>(() => ({
    categoryId: this._categoryFilter() ?? undefined,
    supplierId: this._supplierFilter() ?? undefined,
  }));

  constructor() {
    const structured$ = toObservable(this.structuredFilters);
    // Only the free-text search is debounced — it is the one signal that
    // changes on every keystroke; category/supplier/status flip once per
    // click and should refetch immediately.
    const search$ = toObservable(this._searchTerm).pipe(
      debounceTime(300),
      map(term => term.trim() || undefined),
    );

    combineLatest([structured$, search$])
      .pipe(
        // Skip the initial combination (default, filter-less state): the
        // first load is triggered explicitly by the page's ngOnInit instead,
        // so it isn't delayed by search$'s debounce on cold start.
        skip(1),
        takeUntilDestroyed(),
      )
      .subscribe(([filters, search]) =>
        this.loadProducts({ ...filters, search }),
      );

    toObservable(this.summaryFilters)
      .pipe(skip(1), takeUntilDestroyed())
      .subscribe(filters => this.loadProductsSummary(filters));
  }

  setSearchTerm(term: string): void {
    this._searchTerm.set(term);
  }

  setCategoryFilter(categoryId: string | null): void {
    this._categoryFilter.set(categoryId);
  }

  setSupplierFilter(supplierId: string | null): void {
    this._supplierFilter.set(supplierId);
  }

  setStockStatusFilter(status: StockStatus | null): void {
    this._stockStatusFilter.set(
      this._stockStatusFilter() === status ? null : status,
    );
  }

  loadProducts(params?: GetProductsParams): void {
    this.store.dispatch(ProductActions.loadProducts({ params }));
  }

  loadProductsSummary(params?: GetProductsSummaryParams): void {
    this.store.dispatch(ProductActions.loadProductsSummary({ params }));
  }

  loadProduct(id: string): void {
    this.store.dispatch(ProductActions.loadProduct({ id }));
  }

  createProduct(request: CreateProductRequest): void {
    this.store.dispatch(ProductActions.createProduct({ request }));
  }

  saveProductEdit(id: string, request: UpdateProductRequest): void {
    this.store.dispatch(ProductActions.saveProductEdit({ id, request }));
  }

  disableProduct(id: string): void {
    this.store.dispatch(ProductActions.disableProduct({ id }));
  }

  enableProduct(id: string): void {
    this.store.dispatch(ProductActions.enableProduct({ id }));
  }

  removeProductVariant(variantId: string): void {
    this.store.dispatch(ProductActions.removeProductVariant({ variantId }));
  }

  clearSelectedProduct(): void {
    this.store.dispatch(ProductActions.clearSelectedProduct());
  }

  setSelectedLabelProducts(products: Product[]): void {
    this.store.dispatch(ProductActions.setSelectedLabelProducts({ products }));
  }

  clearSelectedLabelProducts(): void {
    this.store.dispatch(ProductActions.clearSelectedLabelProducts());
  }
}
