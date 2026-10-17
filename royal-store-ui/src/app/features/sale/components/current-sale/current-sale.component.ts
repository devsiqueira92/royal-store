import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { SaleLineItem } from '../../interfaces/sale-line-item.interface';
import { DiscountMode } from '../../interfaces/sale-totals.interface';
import { lineItemTaxRate } from '../../interfaces/sale-tax.util';
import { SaleLineItemComponent } from '../sale-line-item/sale-line-item.component';
import { SaleSummaryComponent } from '../sale-summary/sale-summary.component';
import { CheckoutDialogComponent } from '../checkout-dialog/checkout-dialog.component';
import { CheckoutResult } from '../../interfaces/checkout.interface';
import { CashShift } from '@app/features/cash-shift/interfaces/cash-shift.interface';
import { toSignal } from '@angular/core/rxjs-interop';
import { CashShiftFacade } from '@app/features/cash-shift/facade/cash-shift.facade';
import { DialogManagerService } from '@app/shared/services/dialog.service';
import { OpenCashShiftComponent } from '@app/features/cash-shift/components/open-cash-shift/open-cash-shift.component';

@Component({
  selector: 'app-current-sale',
  imports: [
    CurrencyPipe,
    TranslatePipe,
    ButtonModule,
    SaleLineItemComponent,
    SaleSummaryComponent,
    CheckoutDialogComponent,
  ],
  templateUrl: './current-sale.component.html',
  styleUrl: './current-sale.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CurrentSaleComponent {
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly dialogManagerService = inject(DialogManagerService);
  items = input<SaleLineItem[]>([]);
  sellerName = input<string>('');
  saleRef = input<string>('');

  quantityChange = output<{ variantId: string; quantity: number }>();
  removeItem = output<string>();
  clear = output<void>();
  charge = output<CheckoutResult>();

  protected readonly discountValue = signal(0);
  protected readonly discountMode = signal<DiscountMode>('amount');
  protected readonly checkoutOpen = signal(false);

  private readonly current = toSignal(this.cashShiftFacade.current$, {
    initialValue: null as CashShift | null,
  });

  protected readonly subtotal = computed(() =>
    this.items().reduce(
      (total, item) => total + item.unitPrice * item.quantity,
      0,
    ),
  );

  protected readonly discountAmount = computed(() => {
    const value = this.discountValue();
    const subtotal = this.subtotal();
    const raw =
      this.discountMode() === 'percent' ? (subtotal * value) / 100 : value;
    return Math.min(Math.max(raw, 0), subtotal);
  });

  private readonly netSubtotal = computed(
    () => this.subtotal() - this.discountAmount(),
  );

  protected readonly tax = computed(() => {
    const subtotal = this.subtotal();
    if (subtotal <= 0) {
      return 0;
    }

    const discount = this.discountAmount();

    const rawTotal = this.items().reduce((total, item) => {
      const lineSubtotal = item.unitPrice * item.quantity;
      const lineNet = lineSubtotal - discount * (lineSubtotal / subtotal);

      const lineTax = (lineNet * lineItemTaxRate(item)) / 100;
      return total + Math.round(lineTax * 100) / 100;
    }, 0);

    return Math.round(rawTotal * 100) / 100;
  });

  protected readonly taxPercent = computed(() => {
    const net = this.netSubtotal();
    return net > 0 ? (this.tax() / net) * 100 : 0;
  });

  protected readonly total = computed(() => this.netSubtotal() + this.tax());

  protected onQuantityChange(variantId: string, quantity: number): void {
    this.quantityChange.emit({ variantId, quantity });
  }

  protected onConfirmed(result: CheckoutResult): void {
    this.checkoutOpen.set(false);
    this.charge.emit(result);
  }

  protected checkoutModalOpen() {
    if (!this.current()) {
      this.dialogManagerService.open(OpenCashShiftComponent, {
        showHeader: true,
        closable: true,
        closeOnEscape: true,
        dismissableMask: false,
        width: '440px',
        height: 'auto',
      });
      return;
    }
    this.checkoutOpen.set(true);
  }
}
