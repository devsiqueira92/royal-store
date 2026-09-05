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
import { TaxProfileTemplateFacade } from '@app/features/tax-profile-template/facade/tax-profile-template.facade';
import { TaxProfileTemplate } from '@app/features/tax-profile-template/interfaces/tax-profile-template.interface';
import { ProductDetailComponent } from '../product-detail/product-detail.component';
import { ProductTaxesComponent } from '../product-taxes/product-taxes.component';
import { ProductFooterComponent } from '../product-footer/product-footer.component';
import { ProductVariantsComponent } from '../product-variants/product-variants.component';
import { toggleControl } from '@app/shared/utils/toggle-control.utils';
import { ProductLogisticsComponent } from '../product-logistics/product-logistics.component';

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

  constructor() {
    effect(() => {
      this.formGroup.patchValue({ ...this.selectedProduct() });
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
      basePrice: new FormControl(0, [Validators.required, Validators.min(0)]),
      baseCostPrice: new FormControl(null, [Validators.min(0)]),
      unitOfMeasure: new FormControl('UN', [Validators.maxLength(6)]),
      isActive: new FormControl(true),

      stockTrack: new FormControl(true),
      taxProfileTemplateId: new FormControl(null, [Validators.required]),
      ncm: new FormControl(null, [Validators.maxLength(10)]),
      origin: new FormControl('0'),
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
