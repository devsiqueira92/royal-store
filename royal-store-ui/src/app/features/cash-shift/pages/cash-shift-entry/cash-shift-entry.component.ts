import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { combineLatest, filter, take } from 'rxjs';
import { CashShiftFacade } from '../../facade/cash-shift.facade';
import { OpenCashShiftComponent } from '../../components/open-cash-shift/open-cash-shift.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [OpenCashShiftComponent],
  templateUrl: './cash-shift-entry.component.html',
  styleUrl: './cash-shift-entry.component.scss',
})
export class CashShiftEntryComponent implements OnInit {
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly showOpen = signal(false);

  ngOnInit(): void {
    this.cashShiftFacade.loadCurrent();

    combineLatest([
      this.cashShiftFacade.currentLoaded$,
      this.cashShiftFacade.current$,
    ])
      .pipe(
        filter(([loaded]) => loaded),
        take(1),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(([, cashShift]) => {
        if (cashShift) {
          this.router.navigate(['/cash-shift/management'], {
            replaceUrl: true,
          });
          return;
        }
        this.showOpen.set(true);
      });
  }

  protected onOpened(): void {
    this.router.navigate(['/sales']);
  }
}
