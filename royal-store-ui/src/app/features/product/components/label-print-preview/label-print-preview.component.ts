import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  signal,
} from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import {
  LABEL_FIELD_DEFINITIONS,
  LabelFieldKey,
  LabelProduct,
} from '../../interfaces/label-print.interface';
import { LabelBarcodeComponent } from './label-barcode/label-barcode.component';
import { MOCK_LABEL_PRODUCTS } from './label-print-preview.mock';

// Fixed physical sheet spec (keep in sync with the `mm` values in
// label-print-preview.component.scss, which can't read these constants).
const PAGE_HEIGHT_MM = 279;
const MARGIN_TOP_MM = 12.7;
const LABEL_HEIGHT_MM = 12.7;
const ROW_GAP_MM = 0;
const COLUMNS = 6;

const MIN_ZOOM = 0.3;
const MAX_ZOOM = 1.5;
const ZOOM_STEP = 0.1;

@Component({
  selector: 'app-label-print-preview',
  imports: [TranslatePipe, CurrencyPipe, ButtonComponent, LabelBarcodeComponent],
  templateUrl: './label-print-preview.component.html',
  styleUrl: './label-print-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelPrintPreviewComponent {
  products = input<LabelProduct[]>(MOCK_LABEL_PRODUCTS);
  fields = input<LabelFieldKey[]>(['name', 'price', 'barcode', 'sku']);

  protected readonly availableFields = LABEL_FIELD_DEFINITIONS;

  // Interactive from here on: the `fields` input only seeds the starting
  // selection, the checkboxes in the toolbar drive the live preview.
  protected readonly activeFields = signal<LabelFieldKey[]>(this.fields());

  protected readonly zoom = signal(1);
  protected readonly zoomPercent = computed(() =>
    Math.round(this.zoom() * 100),
  );

  private readonly rowsPerPage = Math.max(
    1,
    Math.floor((PAGE_HEIGHT_MM - MARGIN_TOP_MM + ROW_GAP_MM) / (LABEL_HEIGHT_MM + ROW_GAP_MM)),
  );
  protected readonly labelsPerPage = this.rowsPerPage * COLUMNS;

  protected readonly pages = computed<LabelProduct[][]>(() => {
    const items = this.products();
    const perPage = this.labelsPerPage;
    const pages: LabelProduct[][] = [];

    for (let i = 0; i < items.length; i += perPage) {
      pages.push(items.slice(i, i + perPage));
    }

    return pages;
  });

  protected isFieldActive(key: LabelFieldKey): boolean {
    return this.activeFields().includes(key);
  }

  protected toggleField(key: LabelFieldKey): void {
    this.activeFields.update(current =>
      current.includes(key)
        ? current.filter(field => field !== key)
        : [...current, key],
    );
  }

  protected zoomIn(): void {
    this.zoom.update(value => Math.min(MAX_ZOOM, round1(value + ZOOM_STEP)));
  }

  protected zoomOut(): void {
    this.zoom.update(value => Math.max(MIN_ZOOM, round1(value - ZOOM_STEP)));
  }

  protected print(): void {
    window.print();
  }
}

function round1(value: number): number {
  return Math.round(value * 10) / 10;
}
