import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { SaleFacade } from '../../facade/sale.facade';
import { SaleReceipt } from '../../interfaces/sale-receipt.interface';

@Component({
  selector: 'app-fiscal-receipt-preview',
  imports: [CurrencyPipe, DecimalPipe, TranslatePipe, ButtonComponent],
  templateUrl: './fiscal-receipt-preview.component.html',
  styleUrl: './fiscal-receipt-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FiscalReceiptPreviewComponent {
  private readonly saleFacade = inject(SaleFacade);

  saleId = input.required<string>();

  closed = output<void>();

  protected readonly receipt = toSignal(this.saleFacade.saleReceipt$, {
    initialValue: null as SaleReceipt | null,
  });
  protected readonly isLoading = toSignal(
    this.saleFacade.isLoadingSaleReceipt$,
    { initialValue: false },
  );
  private readonly loadError = toSignal(this.saleFacade.error$, {
    initialValue: null as string | null,
  });

  // Loading settled with no receipt to show — the cupom stays unloaded and the
  // template falls back to an error state instead of rendering half-filled data.
  protected readonly failed = computed(
    () => !this.isLoading() && !this.receipt() && !!this.loadError(),
  );

  // Tax is informational only (not folded into the total), so its share is
  // expressed over the total actually charged — same basis as the summary.
  protected readonly taxPercent = computed(() => {
    const receipt = this.receipt();
    if (!receipt || receipt.totalAmount <= 0) {
      return 0;
    }

    return (receipt.taxAmount / receipt.totalAmount) * 100;
  });

  constructor() {
    effect(() => this.saleFacade.loadSaleReceipt(this.saleId()));
    inject(DestroyRef).onDestroy(() => this.saleFacade.clearSaleReceipt());
  }

  protected print(): void {
    window.print();
  }
}
