import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { SkeletonModule } from 'primeng/skeleton';
import { PlainDatePipe } from '@app/shared/pipes/plain-date.pipe';
import { ExpensesByCategoryReport } from '../../interfaces/expense.interface';
import { compactAmount, seriesColor } from '../../utils/expense.util';
import { ChartLegendComponent, ChartLegendItem } from '../chart-legend/chart-legend.component';

/** One stacked segment of a month's bar. */
interface ReportSegment {
  readonly categoryId: string;
  readonly name: string;
  readonly color: string;
  readonly value: number;
  readonly heightPercentage: number;
}

interface ReportColumn {
  readonly month: string;
  readonly total: number;
  readonly totalLabel: string;
  readonly heightPercentage: number;
  readonly segments: readonly ReportSegment[];
}

/**
 * Expenses by category over a run of competence months — the management P&L.
 *
 * Presentational, and deliberately a chart *and* a table: the bars show the
 * shape, the table carries the values, so nothing is readable by colour alone
 * and a screen reader gets the real figures. A parent category's row already
 * includes every descendant's, which is why no row is nested here.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-category-report',
  imports: [
    CurrencyPipe,
    DecimalPipe,
    TranslatePipe,
    SkeletonModule,
    PlainDatePipe,
    ChartLegendComponent,
  ],
  templateUrl: './expense-category-report.component.html',
  styleUrl: './expense-category-report.component.scss',
})
export class ExpenseCategoryReportComponent {
  readonly report = input<ExpensesByCategoryReport | null>(null);
  readonly loading = input<boolean>(false);

  /** The categories in legend order, each with the colour its bars use. */
  protected readonly series = computed(() =>
    (this.report()?.categories ?? []).map((category, index) => ({
      id: category.categoryId,
      name: category.name,
      color: seriesColor(category.color, index),
    })),
  );

  protected readonly legend = computed<ChartLegendItem[]>(() =>
    this.series().map((item) => ({ id: item.id, label: item.name, color: item.color })),
  );

  /**
   * One column per month. Bar heights are scaled against the tallest month, not
   * against each month's own total — otherwise every month would be full height
   * and the chart would say nothing about which month cost more.
   */
  protected readonly columns = computed<readonly ReportColumn[]>(() => {
    const report = this.report();
    if (!report) {
      return [];
    }

    const peak = Math.max(...report.totals, 0);
    const colors = this.series();

    return report.months.map((month, monthIndex) => {
      const total = report.totals[monthIndex] ?? 0;

      return {
        month,
        total,
        totalLabel: compactAmount(total),
        heightPercentage: peak > 0 ? (total / peak) * 100 : 0,
        segments: report.categories
          .map((category, categoryIndex) => ({
            categoryId: category.categoryId,
            name: category.name,
            color: colors[categoryIndex]?.color ?? seriesColor(category.color, categoryIndex),
            value: category.values[monthIndex] ?? 0,
            // Within the bar, each segment is its share of that month.
            heightPercentage: total > 0 ? ((category.values[monthIndex] ?? 0) / total) * 100 : 0,
          }))
          .filter((segment) => segment.value > 0),
      };
    });
  });

  /** The last month of the range, whose share column the table shows. */
  protected readonly lastMonth = computed(() => {
    const months = this.report()?.months ?? [];
    return months.length ? months[months.length - 1] : null;
  });

  protected readonly monthCount = computed(() => this.report()?.months.length ?? 0);
}
