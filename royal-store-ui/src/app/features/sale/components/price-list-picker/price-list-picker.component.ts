import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { RadioButtonModule } from 'primeng/radiobutton';
import { SkeletonModule } from 'primeng/skeleton';
import { SalePriceListOption } from '../../interfaces/sale-price-list-option.interface';

/** Radio list of the price lists a sale can be charged on, each with the cart's subtotal on it. */
@Component({
  selector: 'app-price-list-picker',
  imports: [CurrencyPipe, FormsModule, TranslatePipe, RadioButtonModule, SkeletonModule],
  templateUrl: './price-list-picker.component.html',
  styleUrl: './price-list-picker.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PriceListPickerComponent {
  options = input<SalePriceListOption[]>([]);
  selectedId = input<string | null>(null);
  loading = input<boolean>(false);

  selected = output<string>();

  protected select(id: string): void {
    if (id !== this.selectedId()) {
      this.selected.emit(id);
    }
  }
}
