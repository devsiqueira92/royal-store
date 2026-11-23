import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  DOCUMENT,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { LabelElement, LabelSheetSpec } from '../../interfaces/label-format.interface';
import { computeLabelSheetLayout } from '../../utils/label-sheet-layout.util';
import {
  labelCellStyle,
  LabelPrintCell,
  LabelPrintItem,
  paginateLabelItems,
  toLabelPrintElements,
} from '../../utils/label-print.util';
import { LabelBarcodeComponent } from '../label-print-preview/label-barcode/label-barcode.component';

const MIN_ZOOM = 0.3;
const MAX_ZOOM = 1.5;
const ZOOM_STEP = 0.1;

const round1 = (value: number): number => Math.round(value * 10) / 10;

interface PrintSheet {
  key: number;
  cells: LabelPrintCell[];
}

/**
 * Presentational: the chosen label format drawn at real size (mm), one sheet per
 * page, every slot filled with the next product. Without products it shows the
 * empty grid so the format can still be checked. Prints with the sheet's own paper size.
 */
@Component({
  selector: 'app-label-sheet-print',
  imports: [TranslatePipe, ButtonComponent, LabelBarcodeComponent],
  templateUrl: './label-sheet-print.component.html',
  styleUrl: './label-sheet-print.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelSheetPrintComponent {
  spec = input.required<LabelSheetSpec>();
  elements = input.required<LabelElement[]>();
  items = input.required<LabelPrintItem[]>();

  protected readonly zoom = signal(0.8);
  protected readonly zoomPercent = computed(() => Math.round(this.zoom() * 100));

  private readonly layout = computed(() => computeLabelSheetLayout(this.spec()));

  /** Slots that fit on the paper; overflowing columns are left out of printing. */
  private readonly usableCells = computed(() => this.layout().cells.filter((c) => !c.overflows));

  protected readonly labelsPerPage = computed(() => this.usableCells().length);

  protected readonly sheetStyle = computed(() => ({
    width: `${this.spec().paperWidth}mm`,
    height: `${this.spec().paperHeight}mm`,
  }));

  protected readonly sheets = computed<PrintSheet[]>(() => {
    const cells = this.usableCells();
    const elements = this.elements();
    const items = this.items();
    // No products yet: one empty sheet, just to show the format.
    const pages = items.length ? paginateLabelItems(items, cells.length) : [[]];

    return pages.map((page, pageIndex) => ({
      key: pageIndex,
      cells: cells.map((cell, i) => {
        const item = page[i];
        return {
          key: `${cell.row}-${cell.column}`,
          style: labelCellStyle(cell),
          elements: item ? toLabelPrintElements(elements, item.product) : [],
        };
      }),
    }));
  });

  protected readonly pageCount = computed(() => (this.items().length ? this.sheets().length : 0));

  constructor() {
    // `@page` cannot be bound per element: keep one rule in <head> with this format's
    // paper size. Added last, it wins over the fixed Letter size in custom.scss.
    const document = inject(DOCUMENT);
    const pageStyle = document.createElement('style');
    document.head.appendChild(pageStyle);
    inject(DestroyRef).onDestroy(() => pageStyle.remove());

    effect(() => {
      const { paperWidth, paperHeight } = this.spec();
      pageStyle.textContent =
        paperWidth > 0 && paperHeight > 0
          ? `@media print { @page { size: ${paperWidth}mm ${paperHeight}mm; margin: 0; } }`
          : '';
    });
  }

  protected zoomIn(): void {
    this.zoom.update((value) => Math.min(MAX_ZOOM, round1(value + ZOOM_STEP)));
  }

  protected zoomOut(): void {
    this.zoom.update((value) => Math.max(MIN_ZOOM, round1(value - ZOOM_STEP)));
  }

  protected print(): void {
    window.print();
  }
}
