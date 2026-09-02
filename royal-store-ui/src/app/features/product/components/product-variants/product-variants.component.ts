import { Component, effect, inject, input } from '@angular/core';
import {
  AbstractControl,
  FormArray,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import {
  Product,
  ProductFormControls,
  ProductVariant,
  ProductVariantFormControls,
} from '../../interfaces/product.interface';
import { TranslatePipe } from '@ngx-translate/core';
import { ProductVariantFormComponent } from '../product-variant-form/product-variant-form.component';
import { ProductFacade } from '../../facade/product.facade';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, take, tap } from 'rxjs';

type VariantSeed = Partial<{
  [K in keyof ProductVariant]: ProductVariant[K] | null;
}>;

@Component({
  selector: 'app-product-variants',
  imports: [FormLayoutComponent, TranslatePipe, ProductVariantFormComponent],
  templateUrl: './product-variants.component.html',
  styleUrl: './product-variants.component.scss',
})
export class ProductVariantsComponent {
  itemType = input.required<number>();
  formGroup = input.required<FormGroup<ProductFormControls>>();
  variants = input.required<FormArray<FormGroup<ProductVariantFormControls>>>();

  private readonly productFacade = inject(ProductFacade);

  protected readonly selectedVariants = toSignal(
    this.productFacade.productVariants$,
    { initialValue: [] as ProductVariant[] },
  );

  constructor() {
    effect(() => {
      if (this.selectedVariants() === undefined) {
        this.addVariant();
      } else {
        this.populateForm(this.selectedVariants()!);
      }
    });
  }

  protected addVariant(): void {
    const group = this.buildVariantGroup();
    this.applyItemTypeToVariant(group, this.itemType() === 1);
    this.variants().push(group);
    this.setComputedSku(group, this.variants.length - 1);
  }

  protected duplicateVariant(index: number): void {
    const source = this.variants().at(index).getRawValue();
    const group = this.buildVariantGroup({ ...source, id: null });
    this.applyItemTypeToVariant(group, this.itemType() === 1);
    const insertIndex = index + 1;
    this.variants().insert(insertIndex, group);
    this.setComputedSku(group, insertIndex);
  }

  protected removeVariant(index: number): void {
    if (this.variants().length <= 1) {
      return;
    }

    this.variants().removeAt(index);
  }

  private populateForm(variants: ProductVariant[]): void {
    for (const variant of variants) {
      const group = this.buildVariantGroup(variant);
      this.variants().push(group);
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
      this.toggleControl(group.controls[name], !isService);
    }
  }

  private setComputedSku(
    group: FormGroup<ProductVariantFormControls>,
    index: number,
  ): void {
    const referenceCode = this.formGroup().controls.sku.value ?? '';
    group.controls.sku.setValue(this.computeSku(referenceCode, index));
  }

  private computeSku(referenceCode: string, index: number): string {
    return `${referenceCode.trim()}-${index + 1}`;
  }

  private toggleControl(control: AbstractControl, enabled: boolean): void {
    if (enabled) {
      control.enable({ emitEvent: false });
    } else {
      control.disable({ emitEvent: false });
    }
  }

  private buildVariantGroup(
    source?: VariantSeed,
  ): FormGroup<ProductVariantFormControls> {
    return new FormGroup<ProductVariantFormControls>({
      id: new FormControl(source?.id ?? null),
      size: new FormControl(source?.size ?? null, [Validators.maxLength(50)]),
      color: new FormControl(source?.color ?? null, [Validators.maxLength(50)]),
      sku: new FormControl(source?.sku ?? null, [Validators.maxLength(50)]),
      barcode: new FormControl(source?.barcode ?? null, [
        Validators.maxLength(14),
      ]),
      price: new FormControl(source?.price ?? 0, [
        Validators.required,
        Validators.min(0),
      ]),
      costPrice: new FormControl(source?.costPrice ?? null, [
        Validators.min(0),
      ]),

      stockQuantity: new FormControl(source?.stockQuantity ?? 0, [
        Validators.required,
        Validators.min(0),
      ]),
      minStockQuantity: new FormControl(source?.minStockQuantity ?? null, [
        Validators.min(0),
      ]),
    });
  }
}
