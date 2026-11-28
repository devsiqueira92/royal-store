import { computed, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { combineLatest, debounceTime, map, skip } from 'rxjs';
import { Store } from '@ngrx/store';
import { SyncSalesRequest } from '../interfaces/sync-sales-request.interface';
import { RegisterSaleRequest } from '../interfaces/register-sale-request.interface';
import { GetSalesListParams } from '../interfaces/get-sales-list-params.interface';
import { FulfillmentMode } from '../interfaces/checkout.interface';
import { SaleActions } from '../store/sale.actions';
import {
  selectIsSyncing,
  selectLastSyncResult,
  selectError,
  selectPosProducts,
  selectIsLoadingProducts,
  selectSales,
  selectIsLoadingSales,
  selectIsLoadingMoreSales,
  selectSalesHasMore,
  selectSalesLoadMoreFailed,
  selectSaleDetails,
  selectIsLoadingSaleDetails,
  selectSaleReceipt,
  selectIsLoadingSaleReceipt,
  selectIsRegisteringSale,
  selectLastRegisterSaleResult,
  selectPriceListOptions,
  selectIsLoadingPriceListOptions,
  selectCart,
  selectCartItems,
} from '../store/sale.selectors';
import { SaleLineItem } from '../interfaces/sale-line-item.interface';
import { DiscountMode } from '../interfaces/sale-totals.interface';
import { SalePricingItem } from '../interfaces/sale-price-list-option.interface';
import { filterPosProducts } from '../interfaces/pos-product-search.util';

@Injectable({
  providedIn: 'root',
})
export class SaleFacade {
  private store = inject(Store);

  isSyncing$ = this.store.select(selectIsSyncing);
  lastSyncResult$ = this.store.select(selectLastSyncResult);
  error$ = this.store.select(selectError);
  posProducts$ = this.store.select(selectPosProducts);
  isLoadingProducts$ = this.store.select(selectIsLoadingProducts);
  sales$ = this.store.select(selectSales);
  isLoadingSales$ = this.store.select(selectIsLoadingSales);
  isLoadingMoreSales$ = this.store.select(selectIsLoadingMoreSales);
  salesHasMore$ = this.store.select(selectSalesHasMore);
  salesLoadMoreFailed$ = this.store.select(selectSalesLoadMoreFailed);
  saleDetails$ = this.store.select(selectSaleDetails);
  isLoadingSaleDetails$ = this.store.select(selectIsLoadingSaleDetails);
  saleReceipt$ = this.store.select(selectSaleReceipt);
  isLoadingSaleReceipt$ = this.store.select(selectIsLoadingSaleReceipt);
  isRegisteringSale$ = this.store.select(selectIsRegisteringSale);
  lastRegisterSaleResult$ = this.store.select(selectLastRegisterSaleResult);
  priceListOptions$ = this.store.select(selectPriceListOptions);
  isLoadingPriceListOptions$ = this.store.select(selectIsLoadingPriceListOptions);
  cart$ = this.store.select(selectCart);
  cartItems$ = this.store.select(selectCartItems);

  // All active products for the selected category are loaded upfront (no pagination — see
  // loadPosProducts), so search just filters that in-memory list client-side, the same way the
  // barcode scanner already does, instead of round-tripping to the server on every keystroke.
  private readonly _searchTerm = signal('');
  readonly searchTerm = this._searchTerm.asReadonly();

  filteredPosProducts$ = combineLatest([
    this.posProducts$,
    toObservable(this._searchTerm).pipe(debounceTime(300)),
  ]).pipe(
    map(([products, term]) => filterPosProducts(products, term)),
  );

  setSearchTerm(term: string): void {
    this._searchTerm.set(term);
  }

  // Sales history filters: seller/customer name are free text (debounced, like
  // posProducts' search above); date range and fulfillment mode change in one
  // discrete step each, so they refetch immediately — same split as ProductFacade.
  private readonly _sellerNameFilter = signal('');
  private readonly _customerNameFilter = signal('');
  private readonly _soldFromFilter = signal<Date | null>(null);
  private readonly _soldToFilter = signal<Date | null>(null);
  private readonly _fulfillmentModeFilter = signal<FulfillmentMode | null>(null);

  readonly sellerNameFilter = this._sellerNameFilter.asReadonly();
  readonly customerNameFilter = this._customerNameFilter.asReadonly();
  readonly soldFromFilter = this._soldFromFilter.asReadonly();
  readonly soldToFilter = this._soldToFilter.asReadonly();
  readonly fulfillmentModeFilter = this._fulfillmentModeFilter.asReadonly();

  private readonly structuredSalesFilters = computed(() => ({
    soldFrom: toIsoDate(this._soldFromFilter()),
    soldTo: toIsoDate(this._soldToFilter()),
    fulfillmentMode: this._fulfillmentModeFilter() ?? undefined,
  }));

  constructor() {
    const structured$ = toObservable(this.structuredSalesFilters);
    const sellerName$ = toObservable(this._sellerNameFilter).pipe(
      debounceTime(300),
      map(term => term.trim() || undefined),
    );
    const customerName$ = toObservable(this._customerNameFilter).pipe(
      debounceTime(300),
      map(term => term.trim() || undefined),
    );

    combineLatest([structured$, sellerName$, customerName$])
      .pipe(skip(1), takeUntilDestroyed())
      .subscribe(([structured, sellerName, customerName]) =>
        this.loadSales({ ...structured, sellerName, customerName }),
      );
  }

  setSellerNameFilter(sellerName: string): void {
    this._sellerNameFilter.set(sellerName);
  }

  setCustomerNameFilter(customerName: string): void {
    this._customerNameFilter.set(customerName);
  }

  setSoldFromFilter(soldFrom: Date | null): void {
    this._soldFromFilter.set(soldFrom);
  }

  setSoldToFilter(soldTo: Date | null): void {
    this._soldToFilter.set(soldTo);
  }

  setFulfillmentModeFilter(fulfillmentMode: FulfillmentMode | null): void {
    this._fulfillmentModeFilter.set(fulfillmentMode);
  }

  syncSales(request: SyncSalesRequest): void {
    this.store.dispatch(SaleActions.syncSales({ request }));
  }

  clearSyncResult(): void {
    this.store.dispatch(SaleActions.clearSyncResult());
  }

  loadPosProducts(categoryId?: string): void {
    this.store.dispatch(SaleActions.loadPosProducts({ categoryId }));
  }

  /** (Re)starts the sales history with these filters: first page. */
  loadSales(params?: GetSalesListParams): void {
    this.store.dispatch(SaleActions.loadSales({ params }));
  }

  /** Next page of the sales history; ignored while loading or when there is none. */
  loadMoreSales(): void {
    this.store.dispatch(SaleActions.loadMoreSales());
  }

  loadSaleDetails(saleId: string): void {
    this.store.dispatch(SaleActions.loadSaleDetails({ saleId }));
  }

  clearSaleDetails(): void {
    this.store.dispatch(SaleActions.clearSaleDetails());
  }

  loadSaleReceipt(saleId: string): void {
    this.store.dispatch(SaleActions.loadSaleReceipt({ saleId }));
  }

  clearSaleReceipt(): void {
    this.store.dispatch(SaleActions.clearSaleReceipt());
  }

  registerSale(request: RegisterSaleRequest): void {
    this.store.dispatch(SaleActions.registerSale({ request }));
  }

  clearRegisterSaleResult(): void {
    this.store.dispatch(SaleActions.clearRegisterSaleResult());
  }

  loadPriceListOptions(items: SalePricingItem[]): void {
    this.store.dispatch(SaleActions.loadPriceListOptions({ items }));
  }

  setCartItems(items: SaleLineItem[]): void {
    this.store.dispatch(SaleActions.setCartItems({ items }));
  }

  setCartDiscount(value: number, mode: DiscountMode): void {
    this.store.dispatch(SaleActions.setCartDiscount({ value, mode }));
  }

  clearCart(): void {
    this.store.dispatch(SaleActions.clearCart());
  }

  clearPriceListOptions(): void {
    this.store.dispatch(SaleActions.clearPriceListOptions());
  }
}

// DateOnly on the API side (yyyy-MM-dd) — built from local date parts so the
// day doesn't shift across the UTC boundary like toISOString() would.
function toIsoDate(date: Date | null): string | undefined {
  if (!date) {
    return undefined;
  }

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}
