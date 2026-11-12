import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { TagModule } from 'primeng/tag';
import { MockupRegisterComponent } from './mockup-register/mockup-register.component';
import { MockupVariant } from '../../models/landing.model';

interface HourlyBar {
  readonly label: string;
  readonly value: number;
  readonly peak: boolean;
}

interface StockAlert {
  readonly id: string;
  readonly nameKey: string;
  readonly left: number;
  readonly ratio: number;
  readonly severity: 'danger' | 'warn' | 'secondary';
  readonly icon: string;
  readonly statusKey: string;
}

interface PaymentSlice {
  readonly id: string;
  readonly labelKey: string;
  readonly share: number;
  readonly color: string;
}

interface TrendPoint {
  readonly x: number;
  readonly revenue: number;
  readonly expense: number;
}

/**
 * Stylised preview of the product UI, used as the hero visual and inside the
 * "inside the system" tabs. It is deliberately a *simulation*: no live data, no
 * interactions — so it is exposed to assistive tech as a single labelled image
 * (`role="img"`), which keeps the decorative inner markup out of the a11y tree
 * while still describing what the picture shows.
 *
 * Chart conventions follow the project data-viz rules: one axis per chart,
 * thin marks with rounded data ends, recessive grid lines, selective direct
 * labels, a legend whenever more than one series is plotted, and a categorical
 * palette validated for colour-vision deficiency (emerald / violet / amber).
 */
@Component({
  selector: 'app-dashboard-mockup',
  imports: [TranslatePipe, TagModule, MockupRegisterComponent],
  templateUrl: './dashboard-mockup.component.html',
  styleUrl: './dashboard-mockup.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardMockupComponent {
  readonly variant = input<MockupVariant>('overview');
  /** Describes the picture for screen readers; required by `role="img"`. */
  readonly ariaLabel = input.required<string>();

  protected readonly windowTitleKey = computed(() => {
    switch (this.variant()) {
      case 'pos':
        return 'landing.mockup.window.pos';
      case 'inventory':
        return 'landing.mockup.window.inventory';
      case 'finance':
        return 'landing.mockup.window.finance';
      default:
        return 'landing.mockup.window.overview';
    }
  });

  /** Sales per hour, 10h-21h. `peak` drives the single direct label. */
  protected readonly hourlyBars: readonly HourlyBar[] = [
    { label: '10h', value: 38, peak: false },
    { label: '11h', value: 52, peak: false },
    { label: '12h', value: 71, peak: false },
    { label: '13h', value: 64, peak: false },
    { label: '14h', value: 48, peak: false },
    { label: '15h', value: 57, peak: false },
    { label: '16h', value: 76, peak: false },
    { label: '17h', value: 88, peak: false },
    { label: '18h', value: 100, peak: true },
    { label: '19h', value: 83, peak: false },
    { label: '20h', value: 61, peak: false },
    { label: '21h', value: 34, peak: false },
  ];

  protected readonly stockAlerts: readonly StockAlert[] = [
    {
      id: 'a1',
      nameKey: 'landing.mockup.products.p1',
      left: 2,
      ratio: 0.08,
      severity: 'danger',
      icon: 'pi pi-exclamation-triangle',
      statusKey: 'landing.mockup.stock.critical',
    },
    {
      id: 'a2',
      nameKey: 'landing.mockup.products.p2',
      left: 6,
      ratio: 0.24,
      severity: 'warn',
      icon: 'pi pi-exclamation-circle',
      statusKey: 'landing.mockup.stock.low',
    },
    {
      id: 'a3',
      nameKey: 'landing.mockup.products.p3',
      left: 9,
      ratio: 0.36,
      severity: 'warn',
      icon: 'pi pi-exclamation-circle',
      statusKey: 'landing.mockup.stock.low',
    },
    {
      id: 'a4',
      nameKey: 'landing.mockup.products.p4',
      left: 42,
      ratio: 0.84,
      severity: 'secondary',
      icon: 'pi pi-check-circle',
      statusKey: 'landing.mockup.stock.ok',
    },
  ];

  /** The overview panel shows only the three most urgent alerts. */
  protected readonly overviewAlerts = computed(() => this.stockAlerts.slice(0, 3));

  /**
   * Payment mix. Colours come from the validated categorical order
   * (emerald -> violet -> amber) and are paired with a legend plus a value
   * label, so the split never relies on colour alone.
   */
  protected readonly paymentMix: readonly PaymentSlice[] = [
    { id: 'pix', labelKey: 'landing.mockup.payments.pix', share: 46, color: '#047857' },
    { id: 'card', labelKey: 'landing.mockup.payments.card', share: 38, color: '#6D28D9' },
    { id: 'cash', labelKey: 'landing.mockup.payments.cash', share: 16, color: '#B45309' },
  ];

  /** 30-day revenue vs. expense, both in BRL — one shared axis, never dual. */
  protected readonly trend: readonly TrendPoint[] = [
    { x: 0, revenue: 42, expense: 30 },
    { x: 1, revenue: 48, expense: 31 },
    { x: 2, revenue: 45, expense: 29 },
    { x: 3, revenue: 55, expense: 33 },
    { x: 4, revenue: 61, expense: 35 },
    { x: 5, revenue: 58, expense: 34 },
    { x: 6, revenue: 67, expense: 36 },
    { x: 7, revenue: 72, expense: 38 },
    { x: 8, revenue: 69, expense: 37 },
    { x: 9, revenue: 80, expense: 41 },
    { x: 10, revenue: 88, expense: 43 },
    { x: 11, revenue: 95, expense: 45 },
  ];

  protected readonly revenueLine = computed(() => this.toLine('revenue'));
  protected readonly expenseLine = computed(() => this.toLine('expense'));
  protected readonly revenueArea = computed(() => `${this.toLine('revenue')} L220,96 L0,96 Z`);

  /** Hero sparkline: today's running revenue, single series, no legend needed. */
  protected readonly sparkline = computed(() => {
    const points = [6, 9, 7, 12, 11, 16, 14, 19, 23, 21, 26, 30];
    const max = Math.max(...points);

    return points
      .map((value, index) => {
        const x = (index / (points.length - 1)) * 80;
        const y = 24 - (value / max) * 22;
        return `${index === 0 ? 'M' : 'L'}${x.toFixed(1)},${y.toFixed(1)}`;
      })
      .join(' ');
  });

  private toLine(series: 'revenue' | 'expense'): string {
    const max = 100;

    return this.trend
      .map((point, index) => {
        const x = (index / (this.trend.length - 1)) * 220;
        const y = 96 - (point[series] / max) * 88;
        return `${index === 0 ? 'M' : 'L'}${x.toFixed(1)},${y.toFixed(1)}`;
      })
      .join(' ');
  }
}
