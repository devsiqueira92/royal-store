import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ProductVariantFormControls } from '../../interfaces/product.interface';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { ToggleFieldComponent } from '@app/shared/components/form-fields/toggle-field/toggle-field.component';
import { ColorSelectFieldComponent } from '@app/shared/components/form-fields/color-select-field/color-select-field.component';
import { ColorPaletteService } from '@app/shared/services/color-palette.service';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { map, startWith, switchMap } from 'rxjs';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { Sizes } from '../../enums/size.enum';
import { enumToOptions } from '@app/shared/utils/enum-to-options.util';

type VariantStockStatus = 'inStock' | 'lowStock' | 'outOfStock' | 'untracked';

@Component({
  selector: 'app-product-variant-form',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    DecimalPipe,
    InputFieldComponent,
    ToggleFieldComponent,
    SelectFieldComponent,
    ColorSelectFieldComponent,
  ],
  templateUrl: './product-variant-form.component.html',
  styleUrl: './product-variant-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductVariantFormComponent {
  private readonly translateService = inject(TranslateService);
  private readonly colorPalette = inject(ColorPaletteService);
  formGroup = input.required<FormGroup<ProductVariantFormControls>>();
  index = input.required<number>();
  removable = input(true);

  duplicate = output<void>();
  remove = output<void>();

  protected readonly sizes = enumToOptions(Sizes, 'enum.sizes');

  // price/costPrice/minStockQuantity start disabled (driven by the product
  // form's base fields), so raw value is used — FormGroup.value/valueChanges
  // omit disabled controls, which would otherwise blank out the margin calc.
  protected formValues = toSignal(
    toObservable(this.formGroup).pipe(
      switchMap(form =>
        form.valueChanges.pipe(
          startWith(null),
          map(() => form.getRawValue()),
        ),
      ),
    ),
  );

  protected readonly pricingLocked = signal(true);

  protected togglePricingLock(): void {
    const { price, costPrice, minStockQuantity } = this.formGroup().controls;

    if (this.pricingLocked()) {
      price.enable();
      costPrice.enable();
      minStockQuantity.enable();
    } else {
      price.disable();
      costPrice.disable();
      minStockQuantity.disable();
    }

    this.pricingLocked.update(locked => !locked);
  }

  protected variantLabel = computed(() => {
    const values = this.formValues();
    if (!values || !values.size) return '';

    const sizes = this.translateService.instant(`enum.sizes.${values.size}`);
    const colorLabel = this.colorPalette.getLabel(values.color);
    return [sizes, colorLabel].filter(Boolean).join(' · ');
  });

  protected swatchColor = computed(
    () =>
      this.colorPalette.getHex(this.formValues()?.color) ??
      'var(--p-surface-300)',
  );

  protected variantStatus = computed<VariantStockStatus>(() => {
    const values = this.formValues();
    if (!values) return 'untracked';

    const { isStockTracked, stockQuantity, minStockQuantity } = values;

    if (!isStockTracked) {
      return 'untracked';
    }

    const quantity = Number(stockQuantity) ?? 0;

    if (quantity <= 0) {
      return 'outOfStock';
    }

    if (minStockQuantity != null && quantity <= Number(minStockQuantity)) {
      return 'lowStock';
    }

    return 'inStock';
  });

  protected variantStatusLabelKey = computed(() => {
    return `product.form.variant.status.${this.variantStatus()}`;
  });

  protected margin = computed(() => {
    const values = this.formValues();
    if (!values || !values.price || values.costPrice == null) {
      return null;
    }

    return Math.round(((values.price - values.costPrice) / values.price) * 100);
  });
}
