import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  linkedSignal,
  output,
  signal,
} from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { LabelElement } from '../../interfaces/label-format.interface';
import {
  LabelElementChange,
  LabelElementPatch,
  LabelFieldOption,
  LabelSampleProduct,
  MIN_ELEMENT_HEIGHT_MM,
  MIN_ELEMENT_WIDTH_MM,
  MM_PER_PT,
  resolveLabelFieldText,
} from '../../utils/label-field.util';
import { LabelBarcodeComponent } from '../label-print-preview/label-barcode/label-barcode.component';
import {
  computeBarcodeGeometry,
  DEFAULT_BARCODE_VALUE_FONT_PT,
} from '../../utils/label-barcode.util';
import { LabelElementPropertiesComponent } from '../label-element-properties/label-element-properties.component';

/** Canvas width in px the label is scaled to (height follows the aspect ratio). */
const CANVAS_MAX_WIDTH_PX = 420;
const CANVAS_MAX_HEIGHT_PX = 320;
/** Drag, resize and arrow-key moves snap to this grid (mm). */
const SNAP_MM = 0.5;
/** Arrow-key step while Shift is held (mm). */
const BIG_STEP_MM = 2;

interface RenderedElement {
  element: LabelElement;
  /** Palette entry for the element's field; gives it a readable name. */
  option: LabelFieldOption | null;
  index: number;
  text: string;
  /** Live width in mm; sizes the barcode's bars. */
  widthMm: number;
  /** Barcode too narrow to scan once printed. */
  unreadable: boolean;
  style: Record<string, string>;
}

/** Live geometry of the element being dragged/resized, in mm. */
interface DragPreview {
  index: number;
  x: number;
  y: number;
  width: number;
  height: number;
}

/** `resize-x`: right edge, `resize-y`: bottom edge, `resize-xy`: bottom-right corner. */
type DragMode = 'move' | 'resize-x' | 'resize-y' | 'resize-xy';

const HANDLE_MODES: Record<string, DragMode> = {
  'resize-x': 'resize-x',
  'resize-y': 'resize-y',
  'resize-xy': 'resize-xy',
};

interface DragState {
  mode: DragMode;
  index: number;
  pointerId: number;
  startClientX: number;
  startClientY: number;
  origin: DragPreview;
}

const snap = (value: number): number => Math.round(value / SNAP_MM) * SNAP_MM;
const round2 = (value: number): number => Math.round(value * 100) / 100;

/**
 * Presentational: product fields palette + scaled drawing of one label with its elements.
 * Click an element to select it (properties panel below), drag it to move, drag its right
 * edge, bottom edge or corner to resize. Every edit goes to the parent through `elementChanged`.
 */
@Component({
  selector: 'app-label-editor',
  imports: [
    TranslatePipe,
    DecimalPipe,
    LabelBarcodeComponent,
    LabelElementPropertiesComponent,
  ],
  templateUrl: './label-editor.component.html',
  styleUrl: './label-editor.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelEditorComponent {
  labelWidth = input.required<number>();
  labelHeight = input.required<number>();
  elements = input.required<LabelElement[]>();
  fieldOptions = input.required<LabelFieldOption[]>();
  sampleProduct = input.required<LabelSampleProduct>();
  /** True when the sample is the built-in mock, not a product picked by the user. */

  fieldToggled = output<LabelFieldOption>();
  /** Emitted once per drag/resize (on release), per arrow-key press and per property edit. */
  elementChanged = output<LabelElementChange>();

  /**
   * A `computed` so it only notifies when the count really changes: a plain source
   * function would rerun the linked computation on every element edit.
   */
  private readonly elementCount = computed(() => this.elements().length);

  /**
   * Kept across edits of the selected element (position, width, font…);
   * cleared only when an element is added or removed, since indexes shift.
   */
  protected readonly selectedIndex = linkedSignal<number, number | null>({
    source: this.elementCount,
    computation: (count, previous) =>
      previous && previous.source === count ? previous.value : null,
  });

  protected readonly selectedElement = computed(() => {
    const index = this.selectedIndex();
    return index == null ? null : (this.elements()[index] ?? null);
  });

  private readonly drag = signal<DragState | null>(null);
  /** Live geometry while dragging; committed through `elementChanged` on release. */
  protected readonly dragPreview = signal<DragPreview | null>(null);

  protected readonly productFields = computed(() =>
    this.fieldOptions().filter(o => o.group === 'product'),
  );

  protected readonly priceFields = computed(() =>
    this.fieldOptions().filter(o => o.group === 'price'),
  );

  private readonly activeFields = computed(
    () => new Set(this.elements().map(e => e.field)),
  );

  private readonly optionsByField = computed(
    () => new Map(this.fieldOptions().map(o => [o.field, o] as const)),
  );

  /** Readable name source for the properties panel (e.g. the price list's name). */
  protected readonly selectedOption = computed(() => {
    const element = this.selectedElement();
    return element ? (this.optionsByField().get(element.field) ?? null) : null;
  });

  /** Bar sizing of the selected barcode, for the properties panel's readability hint. */
  protected readonly selectedBarcode = computed(() => {
    const element = this.selectedElement();
    if (element?.type !== 'barcode') {
      return null;
    }
    const text = resolveLabelFieldText(element.field, this.sampleProduct());
    return computeBarcodeGeometry(text, element.width);
  });

  /** px per mm. */
  protected readonly scale = computed(() => {
    const w = this.labelWidth();
    const h = this.labelHeight();
    if (w <= 0 || h <= 0) {
      return 0;
    }
    return Math.min(CANVAS_MAX_WIDTH_PX / w, CANVAS_MAX_HEIGHT_PX / h);
  });

  protected readonly hasLabel = computed(() => this.scale() > 0);

  protected readonly canvasStyle = computed(() => ({
    width: `${this.labelWidth() * this.scale()}px`,
    height: `${this.labelHeight() * this.scale()}px`,
  }));

  protected readonly renderedElements = computed<RenderedElement[]>(() => {
    const scale = this.scale();
    const product = this.sampleProduct();
    const preview = this.dragPreview();
    const options = this.optionsByField();

    return this.elements().map((element, index) => {
      const box = preview?.index === index ? preview : element;
      const isBarcode = element.type === 'barcode';
      const text = resolveLabelFieldText(
        element.field,
        product,
        element.showLabel ?? true,
      );
      const fontSize =
        element.fontSize ?? (isBarcode ? DEFAULT_BARCODE_VALUE_FONT_PT : 9);
      return {
        element,
        index,
        option: options.get(element.field) ?? null,
        text,
        widthMm: box.width,
        unreadable:
          isBarcode &&
          computeBarcodeGeometry(text, box.width)?.readable === false,
        style: {
          left: `${box.x * scale}px`,
          top: `${box.y * scale}px`,
          width: `${box.width * scale}px`,
          height: `${box.height * scale}px`,
          'font-size': `${fontSize * MM_PER_PT * scale}px`,
          'font-weight': element.fontWeight === 'bold' ? '700' : '400',
          'text-align': element.alignment ?? (isBarcode ? 'center' : 'left'),
          // Barcodes stack bars and digits in a column: centre them vertically.
          'justify-content': isBarcode
            ? 'center'
            : element.alignment === 'center'
              ? 'center'
              : element.alignment === 'right'
                ? 'flex-end'
                : 'flex-start',
        },
      };
    });
  });

  protected isActive(option: LabelFieldOption): boolean {
    return this.activeFields().has(option.field);
  }

  /** A press on the empty canvas clears the selection. */
  protected onCanvasPointerDown(event: PointerEvent): void {
    if (event.target === event.currentTarget) {
      this.selectedIndex.set(null);
    }
  }

  protected onPointerDown(event: PointerEvent, index: number): void {
    if (event.button !== 0) {
      return;
    }
    const element = this.elements()[index];
    if (!element) {
      return;
    }
    event.preventDefault();
    const target = event.currentTarget as HTMLElement;
    target.setPointerCapture(event.pointerId);
    target.focus();
    this.selectedIndex.set(index);

    const handle = (event.target as HTMLElement).dataset['handle'] ?? '';
    const origin: DragPreview = {
      index,
      x: element.x,
      y: element.y,
      width: element.width,
      height: element.height,
    };
    this.drag.set({
      mode: HANDLE_MODES[handle] ?? 'move',
      index,
      pointerId: event.pointerId,
      startClientX: event.clientX,
      startClientY: event.clientY,
      origin,
    });
    this.dragPreview.set(origin);
  }

  protected onPointerMove(event: PointerEvent): void {
    const drag = this.drag();
    const scale = this.scale();
    if (!drag || drag.pointerId !== event.pointerId || scale <= 0) {
      return;
    }
    const dx = (event.clientX - drag.startClientX) / scale;
    const dy = (event.clientY - drag.startClientY) / scale;
    const { origin, mode } = drag;

    if (mode === 'move') {
      this.dragPreview.set({
        ...origin,
        ...this.clampPosition(drag.index, origin.x + dx, origin.y + dy),
      });
      return;
    }

    this.dragPreview.set({
      ...origin,
      width:
        mode === 'resize-y'
          ? origin.width
          : this.clampWidth(origin.x, origin.width + dx),
      height:
        mode === 'resize-x'
          ? origin.height
          : this.clampHeight(origin.y, origin.height + dy),
    });
  }

  protected onPointerUp(event: PointerEvent): void {
    const drag = this.drag();
    if (!drag || drag.pointerId !== event.pointerId) {
      return;
    }
    const preview = this.dragPreview();
    this.drag.set(null);
    this.dragPreview.set(null);
    if (!preview) {
      return;
    }

    const { origin } = drag;
    const changes: LabelElementPatch = {};
    if (drag.mode === 'move') {
      if (preview.x !== origin.x || preview.y !== origin.y) {
        changes.x = preview.x;
        changes.y = preview.y;
      }
    } else {
      if (preview.width !== origin.width) {
        changes.width = preview.width;
      }
      if (preview.height !== origin.height) {
        changes.height = preview.height;
      }
    }

    this.emitChange(drag.index, changes);
  }

  /** Arrow keys nudge the focused element; Shift moves in bigger steps. */
  protected onKeyDown(event: KeyboardEvent, index: number): void {
    const step = event.shiftKey ? BIG_STEP_MM : SNAP_MM;
    const delta: Record<string, [number, number]> = {
      ArrowLeft: [-step, 0],
      ArrowRight: [step, 0],
      ArrowUp: [0, -step],
      ArrowDown: [0, step],
    };
    const move = delta[event.key];
    const element = this.elements()[index];
    if (!move || !element) {
      return;
    }
    event.preventDefault();
    this.selectedIndex.set(index);
    const next = this.clampPosition(
      index,
      element.x + move[0],
      element.y + move[1],
    );
    if (next.x !== element.x || next.y !== element.y) {
      this.emitChange(index, next);
    }
  }

  protected onPropertiesChanged(changes: LabelElementPatch): void {
    const index = this.selectedIndex();
    if (index != null) {
      this.emitChange(index, changes);
    }
  }

  private emitChange(index: number, changes: LabelElementPatch): void {
    if (Object.keys(changes).length > 0) {
      this.elementChanged.emit({ index, changes });
    }
  }

  /** Snaps to the grid and keeps the whole element inside the label. */
  private clampPosition(
    index: number,
    x: number,
    y: number,
  ): { x: number; y: number } {
    const element = this.elements()[index];
    const maxX = Math.max(this.labelWidth() - element.width, 0);
    const maxY = Math.max(this.labelHeight() - element.height, 0);
    const clamp = (value: number, max: number): number =>
      round2(Math.min(Math.max(snap(value), 0), max));
    return { x: clamp(x, maxX), y: clamp(y, maxY) };
  }

  /** Snaps to the grid, at least the minimum width, never past the label's right edge. */
  private clampWidth(x: number, width: number): number {
    const maxWidth = Math.max(this.labelWidth() - x, MIN_ELEMENT_WIDTH_MM);
    return round2(
      Math.min(Math.max(snap(width), MIN_ELEMENT_WIDTH_MM), maxWidth),
    );
  }

  /** Snaps to the grid, at least the minimum height, never past the label's bottom edge. */
  private clampHeight(y: number, height: number): number {
    const maxHeight = Math.max(this.labelHeight() - y, MIN_ELEMENT_HEIGHT_MM);
    return round2(
      Math.min(Math.max(snap(height), MIN_ELEMENT_HEIGHT_MM), maxHeight),
    );
  }
}
