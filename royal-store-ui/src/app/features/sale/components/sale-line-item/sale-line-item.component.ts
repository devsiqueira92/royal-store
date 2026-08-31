import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
} from '@angular/core';
import { CurrencyPipe, UpperCasePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { ColorPaletteService } from '@app/shared/services/color-palette.service';
import { SaleLineItem } from '../../interfaces/sale-line-item.interface';

@Component({
  selector: 'app-sale-line-item',
  imports: [
    CurrencyPipe,
    UpperCasePipe,
    FormsModule,
    TranslatePipe,
    InputNumberModule,
    ButtonModule,
  ],
  templateUrl: './sale-line-item.component.html',
  styleUrl: './sale-line-item.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SaleLineItemComponent {
  private readonly colorPalette = inject(ColorPaletteService);

  item = input.required<SaleLineItem>();

  quantityChange = output<number>();
  remove = output<void>();

  // Same swatch resolution used by the product variant form.
  protected readonly swatchColor = computed(
    () =>
      this.colorPalette.getHex(this.item().variantColor) ??
      'var(--p-surface-300)',
  );

  protected onQuantityChange(value: number | null): void {
    this.quantityChange.emit(value && value > 0 ? value : 1);
  }
}
