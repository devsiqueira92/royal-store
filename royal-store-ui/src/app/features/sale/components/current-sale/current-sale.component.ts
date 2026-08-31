import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
  signal,
} from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { SelectButtonModule } from 'primeng/selectbutton';
import { SaleLineItem } from '../../interfaces/sale-line-item.interface';
import {
  DiscountMode,
  PaymentMethod,
} from '../../interfaces/sale-totals.interface';
import { SaleLineItemComponent } from '../sale-line-item/sale-line-item.component';
import { SaleSummaryComponent } from '../sale-summary/sale-summary.component';

interface PaymentOption {
  label: string;
  value: PaymentMethod;
}

const TAX_RATE = 0.08;

@Component({
  selector: 'app-current-sale',
  imports: [
    CurrencyPipe,
    FormsModule,
    TranslatePipe,
    ButtonModule,
    SelectButtonModule,
    SaleLineItemComponent,
    SaleSummaryComponent,
  ],
  templateUrl: './current-sale.component.html',
  styleUrl: './current-sale.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CurrentSaleComponent {
  items = input<SaleLineItem[]>([]);
  sellerName = input<string>('');
  saleRef = input<string>('');

  quantityChange = output<{ variantId: string; quantity: number }>();
  removeItem = output<string>();
  clear = output<void>();
  charge = output<number>();

  protected readonly paymentOptions: PaymentOption[] = [
    { label: 'sale.current.payment.cash', value: 'cash' },
    { label: 'sale.current.payment.card', value: 'card' },
    { label: 'sale.current.payment.split', value: 'split' },
  ];

  protected readonly discountValue = signal(0);
  protected readonly discountMode = signal<DiscountMode>('amount');
  protected readonly paymentMethod = signal<PaymentMethod>('card');

  protected readonly taxRate = TAX_RATE;

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
      this.discountMode() === 'percent'
        ? (subtotal * value) / 100
        : value;
    return Math.min(Math.max(raw, 0), subtotal);
  });

  protected readonly tax = computed(
    () => (this.subtotal() - this.discountAmount()) * TAX_RATE,
  );

  protected readonly total = computed(
    () => this.subtotal() - this.discountAmount() + this.tax(),
  );

  protected onQuantityChange(variantId: string, quantity: number): void {
    this.quantityChange.emit({ variantId, quantity });
  }

  protected onCharge(): void {
    this.charge.emit(this.total());
  }
}
