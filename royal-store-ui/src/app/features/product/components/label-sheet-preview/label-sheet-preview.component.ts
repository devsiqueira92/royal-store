import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { LabelSheetLayout } from '../../interfaces/label-format.interface';

/** Presentational: outline drawing of the sheet, its margins and every label slot. Units are mm. */
@Component({
  selector: 'app-label-sheet-preview',
  imports: [TranslatePipe, DecimalPipe],
  templateUrl: './label-sheet-preview.component.html',
  styleUrl: './label-sheet-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelSheetPreviewComponent {
  layout = input.required<LabelSheetLayout>();

  protected readonly spec = computed(() => this.layout().spec);

  protected readonly hasPaper = computed(
    () => this.spec().paperWidth > 0 && this.spec().paperHeight > 0,
  );

  protected readonly viewBox = computed(
    () => `0 0 ${this.spec().paperWidth} ${this.spec().paperHeight}`,
  );

  /** Printable area inside the margins; clamped so a bad margin never yields a negative rect. */
  protected readonly printableArea = computed(() => {
    const s = this.spec();
    return {
      x: s.marginLeft,
      y: s.marginTop,
      width: Math.max(s.paperWidth - s.marginLeft - s.marginRight, 0),
      height: Math.max(s.paperHeight - s.marginTop - s.marginBottom, 0),
    };
  });

  protected readonly labelsPerSheet = computed(
    () => this.layout().cells.length,
  );
}
