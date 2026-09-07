import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
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
import { startWith } from 'rxjs';
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
import { ProductTaxesComponent } from '../product-taxes/product-taxes.component';
import { ProductFooterComponent } from '../product-footer/product-footer.component';
import { ProductVariantsComponent } from '../product-variants/product-variants.component';
import { toggleControl } from '@app/shared/utils/toggle-control.utils';
import { ProductLogisticsComponent } from '../product-logistics/product-logistics.component';
import { TaxProfileTemplateFacade } from '@app/core/tax-profile-template/facade/tax-profile-template.facade';
import { TaxProfileTemplate } from '@app/core/tax-profile-template/interfaces/tax-profile-template.interface';

// Advanced fiscal controls driven by the tax profile template's override matrix.
// `cest` is intentionally excluded — it's a free Product field, not template-driven.

@Component({
  imports: [
    ReactiveFormsModule,
    FormsModule,
    ToggleFieldComponent,
    ItemTypeSelectorComponent,
    ProductDetailComponent,
    ProductTaxesComponent,
    ProductFooterComponent,
    ProductVariantsComponent,
    ProductLogisticsComponent,
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

      stockTrack: new FormControl(true),
      taxProfileTemplateId: new FormControl(null, [Validators.required]),
      ncm: new FormControl(null, [Validators.maxLength(10)]),
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
    });
  }
}
