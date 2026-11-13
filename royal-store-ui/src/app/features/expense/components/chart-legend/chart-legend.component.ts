import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export interface ChartLegendItem {
  readonly id: string;
  readonly label: string;
  readonly color: string;
  /** Draws the swatch hatched, for a series rendered with a texture. */
  readonly hatched?: boolean;
}

/**
 * The legend both expense charts carry.
 *
 * A legend is always present once a chart has more than one series, and every
 * name here also appears in the table below the chart — so identity never rests
 * on colour alone.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-chart-legend',
  imports: [],
  template: `
    <ul class="legend">
      @for (item of items(); track item.id) {
        <li class="legend__item">
          <span
            class="legend__dot"
            [class.legend__dot--hatched]="item.hatched"
            aria-hidden="true"
            [style.--legend-color]="item.color"
          ></span>
          {{ item.label }}
        </li>
      }
    </ul>
  `,
  styleUrl: './chart-legend.component.scss',
})
export class ChartLegendComponent {
  readonly items = input.required<readonly ChartLegendItem[]>();
}
