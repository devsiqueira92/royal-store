import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
} from '@angular/core';
import { CurrencyPipe, UpperCasePipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ColorPaletteService } from '@app/shared/services/color-palette.service';
import { PosProduct } from '../../interfaces/pos-product.interface';

const DEFAULT_CATEGORY_COLOR = 'var(--p-surface-400)';

@Component({
  selector: 'app-product-card',
  imports: [CurrencyPipe, UpperCasePipe, TranslatePipe, ButtonModule],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductCardComponent {
  private readonly colorPalette = inject(ColorPaletteService);

  product = input.required<PosProduct>();

  add = output<PosProduct>();

  protected readonly defaultCategoryColor = DEFAULT_CATEGORY_COLOR;

  // Same swatch resolution used by the product variant form.
  protected readonly swatchColor = computed(
    () =>
      this.colorPalette.getHex(this.product().variantColor) ??
      'var(--p-surface-300)',
  );

  protected onAdd(): void {
    if (!this.product().soldOut) {
      this.add.emit(this.product());
    }
  }
}
