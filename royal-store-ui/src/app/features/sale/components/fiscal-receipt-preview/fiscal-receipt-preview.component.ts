import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
} from '@angular/core';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { FiscalReceiptData } from '../../interfaces/fiscal-receipt.interface';

@Component({
  selector: 'app-fiscal-receipt-preview',
  imports: [CurrencyPipe, DatePipe, DecimalPipe, TranslatePipe, ButtonComponent],
  templateUrl: './fiscal-receipt-preview.component.html',
  styleUrl: './fiscal-receipt-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FiscalReceiptPreviewComponent {
  data = input.required<FiscalReceiptData>();

  closed = output<void>();

  protected readonly shortSaleId = computed(() =>
    this.data().saleId.slice(0, 8).toUpperCase(),
  );

  // Tax is informational only (not folded into the total), so its share is
  // expressed over the total actually charged — same basis as the summary.
  protected readonly taxPercent = computed(() => {
    const { tax, total } = this.data().totals;
    return total > 0 ? (tax / total) * 100 : 0;
  });

  protected print(): void {
    window.print();
  }
}
