import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
} from '@angular/core';
import { CurrencyPipe, PercentPipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { Customer } from '../../interfaces/customer.interface';

/** How many matches are listed at once; past it the cashier refines the search. */
const VISIBLE_RESULTS = 6;

/**
 * Picks who the sale is for — no customer is a valid counter sale. Once one is picked, it shows the
 * cashback this purchase earns them (informational only) and lets the cashier redeem their balance.
 */
@Component({
  selector: 'app-customer-picker',
  imports: [
    CurrencyPipe,
    PercentPipe,
    ReactiveFormsModule,
    TranslatePipe,
    ButtonModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    SwitchComponent,
  ],
  templateUrl: './customer-picker.component.html',
  styleUrl: './customer-picker.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerPickerComponent {
  customers = input.required<Customer[]>();
  search = input.required<FormControl<string>>();
  selected = input<Customer | null>(null);
  /** Tier cashback as a 0..1 rate. */
  cashbackRate = input<number>(0);
  /** Cashback this sale will credit to the selected customer. */
  cashbackEarned = input<number>(0);
  /** Usable balance of the selected customer; null while loading or when there is none. */
  cashbackBalance = input<number | null>(null);
  useCashback = input<boolean>(false);

  customerSelected = output<string | null>();
  cashbackToggled = output<boolean>();

  protected readonly visibleCustomers = computed(() =>
    this.customers().slice(0, VISIBLE_RESULTS),
  );
  protected readonly hiddenCount = computed(() =>
    Math.max(this.customers().length - VISIBLE_RESULTS, 0),
  );

  protected initial(name: string): string {
    return name.trim().charAt(0).toUpperCase();
  }
}
