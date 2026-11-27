import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  computed,
  effect,
  inject,
  input,
  output,
  viewChild,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { PrintService } from '@app/shared/services/print.service';
import { SaleFacade } from '../../facade/sale.facade';
import { cashChange } from '../../interfaces/cash-change.util';
import { SaleReceipt } from '../../interfaces/sale-receipt.interface';

/** 80mm thermal roll; the preview is drawn at the same width so it prints as seen. */
const RECEIPT_PAGE_SIZE = '80mm auto';

/** The registered sale's non-fiscal receipt (cupom não fiscal), ready to print. */
@Component({
  selector: 'app-fiscal-receipt-preview',
  imports: [CurrencyPipe, DecimalPipe, TranslatePipe, ButtonModule],
  templateUrl: './fiscal-receipt-preview.component.html',
  styleUrl: './fiscal-receipt-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FiscalReceiptPreviewComponent {
  private readonly saleFacade = inject(SaleFacade);
  private readonly printService = inject(PrintService);

  saleId = input.required<string>();
  /** Cashback the API credited to the customer on this sale. */
  cashbackEarned = input<number>(0);
  /** Cash handed over at checkout (not stored by the API), printed with its change. */
  cashReceived = input<number | null>(null);

  closed = output<void>();

  private readonly receiptElement =
    viewChild<ElementRef<HTMLElement>>('receiptElement');

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

  protected readonly change = computed(() => {
    const receipt = this.receipt();
    return receipt ? cashChange(this.cashReceived(), receipt.totalAmount) : null;
  });

  constructor() {
    effect(() => this.saleFacade.loadSaleReceipt(this.saleId()));
    inject(DestroyRef).onDestroy(() => this.saleFacade.clearSaleReceipt());
  }

  protected retry(): void {
    this.saleFacade.loadSaleReceipt(this.saleId());
  }

  protected print(): void {
    const element = this.receiptElement()?.nativeElement;
    if (element) {
      this.printService.printElement(element, { pageSize: RECEIPT_PAGE_SIZE });
    }
  }
}
