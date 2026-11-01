import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  effect,
  input,
  viewChild,
} from '@angular/core';
import JsBarcode from 'jsbarcode';

@Component({
  selector: 'app-label-barcode',
  imports: [],
  templateUrl: './label-barcode.component.html',
  styleUrl: './label-barcode.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelBarcodeComponent {
  value = input.required<string>();

  private readonly svgRef =
    viewChild.required<ElementRef<SVGSVGElement>>('svgRef');

  constructor() {
    effect(() => {
      const value = this.value();
      const svg = this.svgRef().nativeElement;

      if (!value) {
        svg.replaceChildren();
        return;
      }

      try {
        JsBarcode(svg, value, {
          format: 'CODE128',
          displayValue: false,
          margin: 0,
          height: 28,
          width: 1.4,
        });
      } catch {
        // Not every product barcode is a well-formed CODE128 value (e.g.
        // mid-edit) — leave the label slot blank rather than throwing.
        svg.replaceChildren();
      }
    });
  }
}
