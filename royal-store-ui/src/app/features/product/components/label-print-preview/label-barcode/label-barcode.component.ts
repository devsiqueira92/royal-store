import {
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  effect,
  input,
  viewChild,
} from '@angular/core';
import JsBarcode from 'jsbarcode';
import { BARCODE_FORMAT, computeBarcodeGeometry } from '../../../utils/label-barcode.util';

/**
 * CODE128 bars as SVG. With `widthMm`, the bars are sized in whole printer dots
 * (see `computeBarcodeGeometry`) and stretched only vertically, so the element's
 * height changes the bar height without touching the bar widths that the scanner reads.
 * `pxPerMm` draws the same geometry on a scaled screen canvas; absent → real mm.
 */
@Component({
  selector: 'app-label-barcode',
  imports: [],
  templateUrl: './label-barcode.component.html',
  styleUrl: './label-barcode.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelBarcodeComponent {
  value = input.required<string>();
  /** Width available to the bars, in mm. Absent → fill the host, keeping proportions. */
  widthMm = input<number | null>(null);
  pxPerMm = input<number | null>(null);

  private readonly svgRef = viewChild.required<ElementRef<SVGSVGElement>>('svgRef');

  private readonly geometry = computed(() => {
    const width = this.widthMm();
    return width == null ? null : computeBarcodeGeometry(this.value(), width);
  });

  /** CSS width of the bars; `null` → 100% of the host. */
  protected readonly svgWidth = computed(() => {
    const geometry = this.geometry();
    if (!geometry) {
      return null;
    }
    const scale = this.pxPerMm();
    return scale ? `${geometry.widthMm * scale}px` : `${geometry.widthMm}mm`;
  });

  constructor() {
    effect(() => {
      const value = this.value();
      const sized = this.widthMm() != null;
      const svg = this.svgRef().nativeElement;

      if (!value) {
        svg.replaceChildren();
        return;
      }

      try {
        // width 1 → one viewBox unit per module, so the CSS width sets the module size.
        JsBarcode(svg, value, {
          format: BARCODE_FORMAT,
          displayValue: false,
          margin: 0,
          height: 100,
          width: 1,
          background: 'transparent',
        });
        svg.removeAttribute('width');
        svg.removeAttribute('height');
        svg.setAttribute('preserveAspectRatio', sized ? 'none' : 'xMidYMid meet');
        svg.setAttribute('shape-rendering', 'crispEdges');
      } catch {
        // Not every product barcode is a well-formed CODE128 value (e.g.
        // mid-edit) — leave the label slot blank rather than throwing.
        svg.replaceChildren();
      }
    });
  }
}
