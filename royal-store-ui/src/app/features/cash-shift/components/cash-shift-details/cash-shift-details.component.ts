import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { filter, map, take } from 'rxjs';
import { CashShiftFacade } from '../../facade/cash-shift.facade';
import { CashShift } from '../../interfaces/cash-shift.interface';
import { CashShiftOverview } from '../../interfaces/cash-shift-overview.interface';
import { CashMovementResponse } from '../../interfaces/cash-movement-response.interface';
import { CloseCashShiftRequest } from '../../interfaces/close-cash-shift-request.interface';
import { CashShiftSummaryComponent } from '../cash-shift-summary/cash-shift-summary.component';
import { CashMovementFormComponent } from '../cash-movement-form/cash-movement-form.component';
import { CashMovementListComponent } from '../cash-movement-list/cash-movement-list.component';
import { CloseShiftFormComponent } from '../close-shift-form/close-shift-form.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-cash-shift-details',
  imports: [
    TranslatePipe,
    CashShiftSummaryComponent,
    CashMovementFormComponent,
    CashMovementListComponent,
    CloseShiftFormComponent,
  ],
  templateUrl: './cash-shift-details.component.html',
  styleUrl: './cash-shift-details.component.scss',
})
export class CashShiftDetailsComponent {
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  // paramMap instead of snapshot: the component is reused when navigating between ids.
  private readonly routeShiftId = toSignal(
    this.route.paramMap.pipe(map(params => params.get('id'))),
    { initialValue: this.route.snapshot.paramMap.get('id') },
  );

  private readonly current = toSignal(this.cashShiftFacade.current$, {
    initialValue: null as CashShift | null,
  });
  private readonly details = toSignal(this.cashShiftFacade.shiftDetails$, {
    initialValue: null,
  });
  private readonly movements = toSignal(this.cashShiftFacade.movements$, {
    initialValue: [] as CashMovementResponse[],
  });
  private readonly movementsShiftId = toSignal(
    this.cashShiftFacade.movementsShiftId$,
    { initialValue: null },
  );
  protected readonly isClosing = toSignal(this.cashShiftFacade.isClosing$, {
    initialValue: false,
  });

  protected readonly cashShift = computed<CashShiftOverview | null>(() => {
    const routeShiftId = this.routeShiftId();
    if (!routeShiftId) {
      return this.current();
    }

    const details = this.details();
    return details?.id === routeShiftId ? details : null;
  });

  // Close and movement endpoints act on the user's current shift, so a shift opened by id is read-only.
  protected readonly editableShift = computed(() =>
    this.routeShiftId() ? null : this.current(),
  );
  protected readonly shiftData = computed<CashShiftOverview | CashShift>(() =>
    this.routeShiftId() ? this.details()! : this.current()!,
  );

  private readonly shiftId = computed(() => this.cashShift()?.id ?? null);

  protected readonly shiftMovements = computed(() =>
    this.movementsShiftId() === this.shiftId() ? this.movements() : [],
  );

  protected readonly isOpen = computed(
    () =>
      this.cashShift()?.closedAt === null ||
      this.cashShift()?.closedAt === undefined,
  );

  protected readonly shiftMeta = computed(
    () => this.cashShift()?.id.slice(-5) ?? '',
  );

  constructor() {
    effect(() => {
      const routeShiftId = this.routeShiftId();
      if (routeShiftId) {
        this.cashShiftFacade.loadShiftDetails(routeShiftId);
      } else {
        this.cashShiftFacade.refreshCurrent();
      }
    });

    effect(() => {
      const shiftId = this.shiftId();
      if (shiftId) {
        this.cashShiftFacade.loadMovements(shiftId);
      }
    });
  }

  protected closeShift(request: CloseCashShiftRequest): void {
    if (this.isClosing()) {
      return;
    }

    this.cashShiftFacade.current$
      .pipe(
        filter(cashShift => !cashShift),
        take(1),
      )
      .subscribe(() => this.router.navigate(['/dashboard']));

    this.cashShiftFacade.closeCashShift(request);
  }
}
