import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  untracked,
} from '@angular/core';
import {
  FormArray,
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { map, startWith } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { TabsModule } from 'primeng/tabs';
import {
  ItemType,
  Product,
  ProductFormControls,
  ProductVariantFormControls,
} from '../../interfaces/product.interface';
import { ToggleFieldComponent } from '@app/shared/components/form-fields/toggle-field/toggle-field.component';
import { ProductFacade } from '../../facade/product.facade';
import { ItemTypeSelectorComponent } from '../item-type-selector/item-type-selector.component';
import { ProductDetailComponent } from '../product-detail/product-detail.component';
import { ProductPricingComponent } from '../product-pricing/product-pricing.component';
import { ProductTaxesComponent } from '../product-taxes/product-taxes.component';
import { ProductFooterComponent } from '../product-footer/product-footer.component';
import { ProductVariantsComponent } from '../product-variants/product-variants.component';
import { toggleControl } from '@app/shared/utils/toggle-control.utils';
import { ProductLogisticsComponent } from '../product-logistics/product-logistics.component';
import { TaxProfileTemplateFacade } from '@app/core/tax-profile-template/facade/tax-profile-template.facade';
import { TaxProfileTemplate } from '@app/core/tax-profile-template/interfaces/tax-profile-template.interface';
import { PriceListManagerComponent } from '../price-list-manager/price-list-manager.component';
import { ProductPriceTableComponent } from '../product-price-table/product-price-table.component';
import {
  PriceList,
  ProductPriceFormControls,
} from '../../interfaces/price-list.interface';
import { buildProductPriceGroup } from '../../utils/product-price-form.util';

type ProductFormTab = 'details' | 'pricing';

// Advanced fiscal controls driven by the tax profile template's override matrix.
// `cest` is intentionally excluded — it's a free Product field, not template-driven.

@Component({
  imports: [
    ReactiveFormsModule,
    FormsModule,
    ToggleFieldComponent,
    ItemTypeSelectorComponent,
    ProductDetailComponent,
    ProductPricingComponent,
    ProductTaxesComponent,
    ProductFooterComponent,
    ProductVariantsComponent,
    ProductLogisticsComponent,
    TranslatePipe,
    TabsModule,
    PriceListManagerComponent,
    ProductPriceTableComponent,
  ],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductFormComponent {
  private readonly productFacade = inject(ProductFacade);
  private readonly taxProfileTemplateFacade = inject(TaxProfileTemplateFacade);
  protected readonly selectedProduct = toSignal(
    this.productFacade.selectedProduct$,
    { initialValue: null as Product | null },
  );

  protected readonly loading = toSignal(this.productFacade.isLoading$, {
    initialValue: false as boolean,
  });

  protected readonly formGroup = this.buildFormGroup();

  protected readonly itemType = toSignal(
    this.formGroup.controls.type.valueChanges.pipe(
      startWith(this.formGroup.controls.type.value),
    ),
    { initialValue: this.formGroup.controls.type.value },
  );

  private readonly selectedTemplateId = toSignal(
    this.formGroup.controls.taxProfileTemplateId.valueChanges.pipe(
      startWith(this.formGroup.controls.taxProfileTemplateId.value),
    ),
    { initialValue: this.formGroup.controls.taxProfileTemplateId.value },
  );

  protected readonly overrideEnabled = toSignal(
    this.formGroup.controls.overrideTaxProfile.valueChanges.pipe(
      startWith(this.formGroup.controls.overrideTaxProfile.value),
    ),
    { initialValue: this.formGroup.controls.overrideTaxProfile.value },
  );

  private readonly allTemplates = toSignal(
    this.taxProfileTemplateFacade.templates$,
    { initialValue: [] as TaxProfileTemplate[] },
  );

  protected readonly taxTemplates = computed(() =>
    this.allTemplates().filter(
      template =>
        template.targetItemType === this.itemType() &&
        (template.isActive || template.id === this.selectedTemplateId()),
    ),
  );

  protected readonly taxHelperText = computed<string | null>(
    () =>
      this.taxTemplates().find(t => t.id === this.selectedTemplateId())
        ?.description ?? null,
  );

  protected get variants(): FormArray<FormGroup<ProductVariantFormControls>> {
    return this.formGroup.controls.variants;
  }

  protected readonly activeTab = signal<ProductFormTab>('details');

  // TEMPORARY: the price lists live here only until their facade/store exist.
  // They are tenant-wide (PriceLists), not part of the product, so the
  // handlers below become facade calls and this signal a toSignal of the
  // store. Ids are generated locally meanwhile, which is also why the footer
  // does not send "prices" yet.
  protected readonly priceLists = signal<PriceList[]>([
    { id: crypto.randomUUID(), name: 'Padrão', isDefault: true, isActive: true },
  ]);

  // Zoneless: FormArray mutations (push/removeAt) are invisible to change
  // detection, so the price table renders from this signal.
  protected readonly priceGroups = signal<FormGroup<ProductPriceFormControls>[]>(
    [],
  );

  protected readonly baseCostPrice = toSignal(
    this.formGroup.controls.baseCostPrice.valueChanges.pipe(
      startWith(this.formGroup.controls.baseCostPrice.value),
    ),
    { initialValue: this.formGroup.controls.baseCostPrice.value },
  );

  private readonly priceValues = toSignal(
    this.formGroup.controls.prices.valueChanges.pipe(
      startWith(null),
      map(() => this.formGroup.controls.prices.getRawValue()),
    ),
    { initialValue: [] },
  );

  /** The tab badge: how many active lists carry a price of their own. */
  protected readonly pricing = computed(() => {
    const values = this.priceValues();
    const activeIds = new Set(
      this.priceLists()
        .filter(list => list.isActive)
        .map(list => list.id),
    );
    const active = values.filter(value => activeIds.has(value.priceListId));

    return {
      total: activeIds.size,
      priced: active.filter(value => value.salePrice != null).length,
      invalid: active.some(
        value =>
          value.salePrice != null &&
          value.minPrice != null &&
          value.minPrice > value.salePrice,
      ),
    };
  });

  // Patch once per loaded item: re-patching on every store emission would
  // replay the type/tax cascades (and reset the tax template) each time a
  // variant is deleted.
  private patchedProductId: string | null | undefined = undefined;

  constructor() {
    effect(() => {
      const product = this.selectedProduct();
      const productId = product?.id ?? null;

      if (productId === this.patchedProductId) {
        return;
      }
      this.patchedProductId = productId;

      this.formGroup.patchValue({ ...product });
    });

    // The store owns the active flag: this mirrors it back into the control
    // so a rejected enable/disable rolls the toggle back.
    effect(() => {
      const isActive = this.selectedProduct()?.isActive ?? true;
      const control = this.formGroup.controls.isActive;

      if (control.value !== isActive) {
        control.setValue(isActive, { emitEvent: false });
      }
    });
    this.formGroup.controls.type.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(type => this.applyItemType(type as ItemType));

    effect(() => this.syncPriceGroups(this.priceLists()));

    // The default list's sale price and basePrice are the same figure shown
    // in two tabs. Both directions compare before writing, so neither loops.
    this.formGroup.controls.basePrice.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(basePrice => {
        const salePrice = this.defaultPriceGroup()?.controls.salePrice;

        if (salePrice && salePrice.value !== basePrice) {
          salePrice.setValue(basePrice);
        }
      });

    this.formGroup.controls.prices.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => {
        const salePrice = this.defaultPriceGroup()?.controls.salePrice;
        const basePrice = this.formGroup.controls.basePrice;

        if (salePrice && salePrice.value !== basePrice.value) {
          basePrice.setValue(salePrice.value);
        }
      });

    this.formGroup.controls.taxProfileTemplateId.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(id => this.applyTaxTemplate(id));

    this.formGroup.controls.overrideTaxProfile.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(override => this.applyFiscalOverride(!!override));

    this.formGroup.controls.isActive.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(isActive => this.applyActiveState(!!isActive));
  }

  /**
   * Persists the isActive toggle right away through
   * `PATCH /api/products/{id}/enable|disable` — it is not part of the
   * batched save. Only reached for an already persisted item, and skipped
   * when the control merely echoes what the store already holds (the patch
   * that fills the form on load).
   */
  private applyActiveState(isActive: boolean): void {
    const product = this.selectedProduct();

    if (!product || product.isActive === isActive) {
      return;
    }

    if (isActive) {
      this.productFacade.enableProduct(product.id);
    } else {
      this.productFacade.disableProduct(product.id);
    }
  }

  protected onTabChange(tab: string | number | undefined): void {
    this.activeTab.set(tab === 'pricing' ? 'pricing' : 'details');
  }

  protected onPriceListAdd(name: string): void {
    this.priceLists.update(lists => [
      ...lists,
      { id: crypto.randomUUID(), name, isDefault: false, isActive: true },
    ]);
  }

  protected onPriceListRename({ id, name }: { id: string; name: string }): void {
    this.priceLists.update(lists =>
      lists.map(list => (list.id === id ? { ...list, name } : list)),
    );
  }

  /** There is always exactly one default, and the default is always active. */
  protected onPriceListDefaultChange(id: string): void {
    this.priceLists.update(lists =>
      lists.map(list => ({
        ...list,
        isDefault: list.id === id,
        isActive: list.isActive || list.id === id,
      })),
    );
  }

  protected onPriceListActiveChange({
    id,
    active,
  }: {
    id: string;
    active: boolean;
  }): void {
    this.priceLists.update(lists =>
      lists.map(list =>
        list.id === id && !list.isDefault ? { ...list, isActive: active } : list,
      ),
    );
  }

  protected onPriceListRemove(id: string): void {
    this.priceLists.update(lists =>
      lists.filter(list => list.id !== id || list.isDefault),
    );
  }

  private defaultPriceGroup(): FormGroup<ProductPriceFormControls> | undefined {
    const defaultId = untracked(this.priceLists).find(list => list.isDefault)?.id;

    return this.formGroup.controls.prices.controls.find(
      group => group.controls.priceListId.value === defaultId,
    );
  }

  /**
   * Keeps one price group per list: groups of removed lists go, new lists get
   * an empty one. An inactive list keeps its group, so switching it back on
   * brings its prices back.
   */
  private syncPriceGroups(lists: PriceList[]): void {
    const prices = this.formGroup.controls.prices;
    const basePrice = this.formGroup.controls.basePrice;

    for (let index = prices.length - 1; index >= 0; index--) {
      const listId = prices.at(index).controls.priceListId.value;

      if (!lists.some(list => list.id === listId)) {
        prices.removeAt(index);
      }
    }

    for (const list of lists) {
      const exists = prices.controls.some(
        group => group.controls.priceListId.value === list.id,
      );

      if (!exists) {
        prices.push(buildProductPriceGroup(list.id));
      }
    }

    // A list just promoted to default either brings its own price along or,
    // having none, takes the product's current sale price.
    const salePrice = this.defaultPriceGroup()?.controls.salePrice;

    if (salePrice && salePrice.value == null) {
      salePrice.setValue(basePrice.value);
    } else if (salePrice && salePrice.value !== basePrice.value) {
      basePrice.setValue(salePrice.value);
    }

    this.priceGroups.set([...prices.controls]);
  }

  protected onItemTypeChange(type: ItemType): void {
    this.formGroup.controls.type.setValue(type);
  }

  private applyItemType(type: ItemType): void {
    const isService = type === 1;
    const c = this.formGroup.controls;

    toggleControl(c.ncm, !isService);
    toggleControl(c.origin, !isService);
    toggleControl(c.nbs, isService);
    toggleControl(c.issCode, isService);
    toggleControl(c.stockTrack, !isService);

    c.taxProfileTemplateId.setValue(null);
    c.stockTrack.setValue(!isService);
    for (const group of this.variants.controls) {
      this.applyItemTypeToVariant(group, isService);
    }
  }

  private applyItemTypeToVariant(
    group: FormGroup<ProductVariantFormControls>,
    isService: boolean,
  ): void {
    const serviceHidden: (keyof ProductVariantFormControls)[] = [
      'size',
      'color',
    ];
    for (const name of serviceHidden) {
      toggleControl(group.controls[name], !isService);
    }
    toggleControl(group.controls.optionName, isService);
  }

  private applyTaxTemplate(id: string | null): void {
    const template = this.taxTemplates().find(t => t.id === id);
    if (!template) {
      return;
    }

    this.formGroup.patchValue({
      defaultCfop: template.defaultCfop || null,
      icmsCst: template.icmsCst || null,
      pisCofinsCst: template.pisCofinsCst || null,
      ivaCst: template.ivaCst || null,
      isSelectiveTax: template.isSelectiveTax,
    });
  }

  private applyFiscalOverride(override: boolean): void {
    const FISCAL_CONTROLS = [
      'defaultCfop',
      'icmsCst',
      'pisCofinsCst',
      'ivaCst',
      'isSelectiveTax',
    ] as const;

    for (const name of FISCAL_CONTROLS) {
      toggleControl(this.formGroup.controls[name], override);
    }
  }

  private buildFormGroup(): FormGroup<ProductFormControls> {
    return new FormGroup<ProductFormControls>({
      id: new FormControl(null),
      type: new FormControl(0, { nonNullable: true }),
      categoryId: new FormControl(null, [Validators.required]),
      name: new FormControl(null, [
        Validators.required,
        Validators.maxLength(120),
      ]),
      sku: new FormControl(null, [
        Validators.required,
        Validators.maxLength(50),
      ]),
      description: new FormControl(null, [Validators.maxLength(500)]),
      basePrice: new FormControl(0, [
        Validators.required,
        Validators.min(0.01),
      ]),
      baseCostPrice: new FormControl(0, [Validators.min(0.01)]),
      unitOfMeasure: new FormControl(null),
      isActive: new FormControl(true),
      supplierId: new FormControl(null),

      stockTrack: new FormControl(true),
      taxProfileTemplateId: new FormControl(null, [Validators.required]),
      ncm: new FormControl('71132000', [Validators.maxLength(10)]),
      origin: new FormControl('0', [Validators.required]),
      nbs: new FormControl({ value: null, disabled: true }),
      issCode: new FormControl({ value: null, disabled: true }),

      overrideTaxProfile: new FormControl(false, { nonNullable: true }),
      cest: new FormControl(null),
      defaultCfop: new FormControl({ value: null, disabled: true }),
      icmsCst: new FormControl({ value: null, disabled: true }),
      pisCofinsCst: new FormControl({ value: null, disabled: true }),
      ivaCst: new FormControl({ value: null, disabled: true }),
      isSelectiveTax: new FormControl(
        { value: false, disabled: true },
        { nonNullable: true },
      ),

      netWeight: new FormControl(null, [Validators.min(0)]),
      grossWeight: new FormControl(null, [Validators.min(0)]),
      width: new FormControl(null, [Validators.min(0)]),
      height: new FormControl(null, [Validators.min(0)]),
      length: new FormControl(null, [Validators.min(0)]),

      variants: new FormArray<FormGroup<ProductVariantFormControls>>([]),
      prices: new FormArray<FormGroup<ProductPriceFormControls>>([]),
    });
  }
}
