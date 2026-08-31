import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-sales-stats',
  imports: [CurrencyPipe, TranslatePipe],
  templateUrl: './sales-stats.component.html',
  styleUrl: './sales-stats.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesStatsComponent {
  // Mocked values for now — wired to real data later.
  todaySales = input<number>(0);
  todayOrders = input<number>(0);
}
