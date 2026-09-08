import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { CashbackEntry } from '../../interfaces/cashback-entry.interface';

@Component({
  selector: 'app-customer-cashback-history',
  imports: [CurrencyPipe, DatePipe, TranslatePipe, TableModule],
  templateUrl: './customer-cashback-history.component.html',
  styleUrl: './customer-cashback-history.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerCashbackHistoryComponent {
  entries = input<CashbackEntry[]>([]);
  isLoading = input<boolean>(false);
  balance = input<number>(0);

  protected readonly availableTotal = computed(() =>
    this.entries()
      .filter(entry => entry.state === 'available')
      .reduce((total, entry) => total + entry.remainingAmount, 0),
  );

  protected isExpired(entry: CashbackEntry): boolean {
    return (
      entry.state === 'available' && new Date(entry.expiresAt) < new Date()
    );
  }
}
