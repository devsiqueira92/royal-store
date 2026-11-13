import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { SkeletonModule } from 'primeng/skeleton';
import { PlainDatePipe } from '@app/shared/pipes/plain-date.pipe';
import { CashFlowProjection } from '../../interfaces/expense.interface';
import { compactAmount } from '../../utils/expense.util';
import { ChartLegendComponent, ChartLegendItem } from '../chart-legend/chart-legend.component';

/** One bar of the projection. */
interface FlowColumn {
  readonly key: string;
  readonly weekStart: string;
  readonly weekEnd: string;
  /** Overdue is shown as its own leading bar, never mixed into a week. */
  readonly kind: 'overdue' | 'week';
  readonly total: number;
  readonly totalLabel: string;
  readonly committedPercentage: number;
  readonly projectedPercentage: number;
}

/**
 * What the store owes over the weeks ahead.
 *
 * The three buckets never hold the same obligation twice: a cycle a template
 * has already generated counts as committed and is excluded from projected, so
 * the bars can be read as one running obligation without double-counting.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-cash-flow',
  imports: [CurrencyPipe, TranslatePipe, SkeletonModule, PlainDatePipe, ChartLegendComponent],
  templateUrl: './expense-cash-flow.component.html',
  styleUrl: './expense-cash-flow.component.scss',
})
export class ExpenseCashFlowComponent {
  private readonly translate = inject(TranslateService);

  readonly projection = input<CashFlowProjection | null>(null);
  readonly loading = input<boolean>(false);

  /** Re-read on a language change so the legend is retranslated with the page. */
  private readonly lang = toSignal(this.translate.onLangChange, { initialValue: null });

  protected readonly legend = computed<ChartLegendItem[]>(() => {
    this.lang();

    return [
      {
        id: 'overdue',
        label: this.translate.instant('expense.report.cashFlow.overdue'),
        color: 'var(--flow-overdue)',
      },
      {
        id: 'committed',
        label: this.translate.instant('expense.report.cashFlow.committed'),
        color: 'var(--p-primary-color)',
      },
      {
        id: 'projected',
        label: this.translate.instant('expense.report.cashFlow.projected'),
        color: 'var(--p-primary-color)',
        hatched: true,
      },
    ];
  });

  protected readonly columns = computed<readonly FlowColumn[]>(() => {
    const projection = this.projection();
    if (!projection) {
      return [];
    }

    const weekTotals = projection.weeks.map((week) => week.committed + week.projected);
    const peak = Math.max(projection.overdue, ...weekTotals, 0);
    const scale = (value: number) => (peak > 0 ? (value / peak) * 100 : 0);

    const overdueColumn: FlowColumn[] = projection.overdue
      ? [
          {
            key: 'overdue',
            weekStart: '',
            weekEnd: '',
            kind: 'overdue',
            total: projection.overdue,
            totalLabel: compactAmount(projection.overdue),
            committedPercentage: scale(projection.overdue),
            projectedPercentage: 0,
          },
        ]
      : [];

    return [
      ...overdueColumn,
      ...projection.weeks.map((week) => ({
        key: week.weekStart,
        weekStart: week.weekStart,
        weekEnd: week.weekEnd,
        kind: 'week' as const,
        total: week.committed + week.projected,
        totalLabel: compactAmount(week.committed + week.projected),
        committedPercentage: scale(week.committed),
        projectedPercentage: scale(week.projected),
      })),
    ];
  });

  protected readonly weekCount = computed(() => this.projection()?.weeks.length ?? 0);
}
