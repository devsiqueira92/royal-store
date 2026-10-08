import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
} from '@angular/core';
import { AsyncPipe, CurrencyPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { CashShiftFacade } from '@app/features/cash-shift/facade/cash-shift.facade';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-sales-stats',
  imports: [CurrencyPipe, TranslatePipe, AsyncPipe],
  templateUrl: './sales-stats.component.html',
  styleUrl: './sales-stats.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesStatsComponent {
  // Mocked values for now — wired to real data later.

  private readonly cashShiftFacade = inject(CashShiftFacade);
  protected readonly current$ = this.cashShiftFacade.current$;
}
