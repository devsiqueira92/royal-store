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
import {
  findTaxProfileTemplate,
  taxProfileTemplatesFor,
} from '../../data/tax-profile-templates';
import { ProductDetailComponent } from '../product-detail/product-detail.component';
import { ProductTaxesComponent } from '../product-taxes/product-taxes.component';
import { ProductFooterComponent } from '../product-footer/product-footer.component';
import { ProductVariantsComponent } from '../product-variants/product-variants.component';
import { toggleControl } from '@app/shared/utils/toggle-control.utils';
import { ProductLogisticsComponent } from '../product-logistics/product-logistics.component';

// Advanced fiscal controls driven by the tax profile template.
const FISCAL_CONTROLS = [
  'cest',
  'defaultCfop',
  'icmsCst',
  'pisCofinsCst',
  'ivaCst',
  'isSelectiveTax',
] as const;

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

  protected readonly selectedProduct = toSignal(
    this.productFacade.selectedProduct$,
    { initialValue: null as Product | null },
  );

  protected readonly loading = toSignal(this.productFacade.isLoading$, {
    initialValue: false as boolean,
  });

  protected readonly formGroup = this.buildFormGroup();

  protected readonly itemType = toSignal(
    this.formGroup.controls.itemType.valueChanges.pipe(
      startWith(this.formGroup.controls.itemType.value),
    ),
    { initialValue: this.formGroup.controls.itemType.value },
  );

  private readonly selectedTemplateCode = toSignal(
    this.formGroup.controls.taxProfileTemplate.valueChanges.pipe(
      startWith(this.formGroup.controls.taxProfileTemplate.value),
    ),
    { initialValue: this.formGroup.controls.taxProfileTemplate.value },
  );

  protected readonly overrideEnabled = toSignal(
    this.formGroup.controls.overrideTaxProfile.valueChanges.pipe(
      startWith(this.formGroup.controls.overrideTaxProfile.value),
    ),
    { initialValue: this.formGroup.controls.overrideTaxProfile.value },
  );

  protected readonly taxTemplates = computed(() =>
    taxProfileTemplatesFor(this.itemType()),
  );

  protected readonly taxHelperText = computed<string | null>(
    () =>
      findTaxProfileTemplate(this.selectedTemplateCode())?.helperText ?? null,
  );

  protected get variants(): FormArray<FormGroup<ProductVariantFormControls>> {
    return this.formGroup.controls.variants;
  }

  constructor() {
    effect(() => this.formGroup.patchValue({ ...this.selectedProduct() }));

    this.formGroup.controls.itemType.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(type => this.applyItemType(type as ItemType));

    // Selecting a tax profile template refreshes the advanced fiscal fields.
    this.formGroup.controls.taxProfileTemplate.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(code => this.applyTaxTemplate(code));

    // Advanced fiscal fields are read-only unless "override" is checked.
    this.formGroup.controls.overrideTaxProfile.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(override => this.applyFiscalOverride(!!override));
  }

  protected onItemTypeChange(type: ItemType): void {
    this.formGroup.controls.itemType.setValue(type);
  }

  private applyItemType(type: ItemType): void {
    const isService = type === 1;
    const c = this.formGroup.controls;

    toggleControl(c.ncm, !isService);
    toggleControl(c.origin, !isService);
    toggleControl(c.nbs, isService);
    toggleControl(c.issCode, isService);

    // Default to the first template for the selected type.
    const [first] = taxProfileTemplatesFor(type);
    c.taxProfileTemplate.setValue(first?.code ?? null);

    // Services carry no size/color/stock/logistics data.
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
      'stockQuantity',
      'minStockQuantity',
    ];
    for (const name of serviceHidden) {
      toggleControl(group.controls[name], !isService);
    }
  }

  private applyTaxTemplate(code: string | null): void {
    const template = findTaxProfileTemplate(code);
    if (!template) {
      return;
    }

    this.formGroup.patchValue({
      cest: template.fiscal.cest || null,
      defaultCfop: template.fiscal.defaultCfop || null,
      icmsCst: template.fiscal.icmsCst || null,
      pisCofinsCst: template.fiscal.pisCofinsCst || null,
      ivaCst: template.fiscal.ivaCst || null,
      isSelectiveTax: template.fiscal.isSelectiveTax,
    });
  }

  private applyFiscalOverride(override: boolean): void {
    for (const name of FISCAL_CONTROLS) {
      toggleControl(this.formGroup.controls[name], override);
    }
  }

  private buildFormGroup(): FormGroup<ProductFormControls> {
    return new FormGroup<ProductFormControls>({
      id: new FormControl(null),
      itemType: new FormControl(0, { nonNullable: true }),
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
      price: new FormControl(0, [Validators.required, Validators.min(0)]),
      costPrice: new FormControl(null, [Validators.min(0)]),
      unitOfMeasure: new FormControl('UN', [Validators.maxLength(6)]),
      isActive: new FormControl(true),

      isStockTracked: new FormControl(true),
      taxProfileTemplate: new FormControl(null, [Validators.required]),
      ncm: new FormControl(null, [Validators.maxLength(10)]),
      origin: new FormControl('0'),
      nbs: new FormControl({ value: null, disabled: true }),
      issCode: new FormControl({ value: null, disabled: true }),

      overrideTaxProfile: new FormControl(false, { nonNullable: true }),
      cest: new FormControl({ value: null, disabled: true }),
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
