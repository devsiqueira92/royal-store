import { CurrencyPipe, DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CashShiftFacade } from '../../facade/cash-shift.facade';
import { CashShiftListItem } from '../../interfaces/cash-shift-list-item.interface';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-cash-shift-list',
  imports: [CurrencyPipe, DatePipe, TableModule, ButtonModule, TranslatePipe],
  templateUrl: './cash-shift-list.component.html',
  styleUrl: './cash-shift-list.component.scss',
})
export class CashShiftListComponent implements OnInit {
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly router = inject(Router);

  protected readonly shifts = toSignal(this.cashShiftFacade.shifts$, {
    initialValue: [] as CashShiftListItem[],
  });
  protected readonly isLoading = toSignal(
    this.cashShiftFacade.isLoadingShifts$,
    { initialValue: false },
  );

  protected readonly dateFormat = 'HH:mm · MMM d, y';

  ngOnInit(): void {
    this.cashShiftFacade.loadShifts();
  }

  protected viewDetails(shift: CashShiftListItem): void {
    this.router.navigate(['/cash-shift', shift.id]);
  }
}
