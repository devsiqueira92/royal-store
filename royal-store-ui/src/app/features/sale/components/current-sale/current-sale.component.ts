import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
} from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { SaleLineItem } from '../../interfaces/sale-line-item.interface';
import { cartSubtotal, cartTax } from '../../interfaces/cart-totals.util';
import { SaleLineItemComponent } from '../sale-line-item/sale-line-item.component';
import { SaleSummaryComponent } from '../sale-summary/sale-summary.component';
import { CashShift } from '@app/features/cash-shift/interfaces/cash-shift.interface';
import { toSignal } from '@angular/core/rxjs-interop';
import { CashShiftFacade } from '@app/features/cash-shift/facade/cash-shift.facade';
import { DialogManagerService } from '@app/shared/services/dialog.service';
import { OpenCashShiftComponent } from '@app/features/cash-shift/components/open-cash-shift/open-cash-shift.component';
import { CurrentSellerFacade } from '@app/core/current-seller/facade/current-seller.facade';
import { take } from 'rxjs';

/** Key that charges the sale from anywhere on the POS, so the cashier never reaches for the mouse. */
const CHECKOUT_SHORTCUT = 'F8';

@Component({
  selector: 'app-current-sale',
  host: {
    '(document:keydown.f8)': 'onCheckoutShortcut($event)',
  },
  imports: [
    CurrencyPipe,
    TranslatePipe,
    ButtonModule,
    SaleLineItemComponent,
    SaleSummaryComponent,
  ],
  templateUrl: './current-sale.component.html',
  styleUrl: './current-sale.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CurrentSaleComponent {
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly dialogManagerService = inject(DialogManagerService);
  private readonly currentSellerFacade = inject(CurrentSellerFacade);

  protected readonly currentSeller = toSignal(
    this.currentSellerFacade.currentSeller$,
    { initialValue: null },
  );

  items = input<SaleLineItem[]>([]);

  quantityChange = output<{ variantId: string; quantity: number }>();
  removeItem = output<string>();
  clear = output<void>();
  /** Asked to go to checkout — only emitted once a cash shift is open. */
  checkout = output<void>();

  protected readonly checkoutShortcut = CHECKOUT_SHORTCUT;
  private cashShiftDialogOpen = false;

  private readonly current = toSignal(this.cashShiftFacade.current$, {
    initialValue: null as CashShift | null,
  });

  protected readonly subtotal = computed(() => cartSubtotal(this.items()));

  // Before any discount: discounts are given at checkout, where the final total is worked out.
  protected readonly tax = computed(() => cartTax(this.items(), 0));

  protected readonly taxPercent = computed(() => {
    const subtotal = this.subtotal();
    return subtotal > 0 ? (this.tax() / subtotal) * 100 : 0;
  });

  protected readonly total = computed(() => this.subtotal());

  protected onQuantityChange(variantId: string, quantity: number): void {
    this.quantityChange.emit({ variantId, quantity });
  }

  /** Same rules as the charge button: nothing happens on an empty cart. */
  protected onCheckoutShortcut(event: Event): void {
    event.preventDefault();

    // Holding the key down repeats it; one press means one trip to checkout.
    if ((event as KeyboardEvent).repeat || this.items().length === 0) {
      return;
    }

    this.goToCheckout();
  }

  protected goToCheckout(): void {
    if (!this.current()) {
      this.openCashShiftDialog();
      return;
    }
    this.checkout.emit();
  }

  private openCashShiftDialog(): void {
    // The shortcut keeps working behind the dialog, so it must not stack a second one.
    if (this.cashShiftDialogOpen) {
      return;
    }

    this.cashShiftDialogOpen = true;
    this.dialogManagerService
      .open(OpenCashShiftComponent, {
        showHeader: true,
        closable: true,
        closeOnEscape: true,
        dismissableMask: false,
        width: '440px',
        height: 'auto',
      })
      .onClose.pipe(take(1))
      .subscribe(() => (this.cashShiftDialogOpen = false));
  }
}
