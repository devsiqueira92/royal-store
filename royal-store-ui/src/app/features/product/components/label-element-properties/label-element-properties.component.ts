import { ChangeDetectionStrategy, Component, effect, input, output } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { SelectButtonModule } from 'primeng/selectbutton';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import {
  LabelElement,
  LabelElementAlignment,
  LabelElementType,
} from '../../interfaces/label-format.interface';
import {
  canSwitchToBarcode,
  LabelElementPatch,
  switchElementType,
  LabelFieldOption,
  MAX_FONT_SIZE_PT,
  MIN_ELEMENT_HEIGHT_MM,
  MIN_ELEMENT_WIDTH_MM,
  MIN_FONT_SIZE_PT,
  minHeightForFont,
} from '../../utils/label-field.util';

interface TypeOption {
  value: LabelElementType;
  icon: string;
  labelKey: string;
}

interface AlignmentOption {
  value: LabelElementAlignment;
  icon: string;
  labelKey: string;
}

const round = (value: number, decimals = 1): number => {
  const factor = 10 ** decimals;
  return Math.round(value * factor) / factor;
};

/**
 * Presentational: font size, alignment and width of one label element.
 * Emits only what changed, already clamped to the label.
 */
@Component({
  selector: 'app-label-element-properties',
  imports: [ReactiveFormsModule, TranslatePipe, SelectButtonModule, InputNumberFieldComponent],
  templateUrl: './label-element-properties.component.html',
  styleUrl: './label-element-properties.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelElementPropertiesComponent {
  element = input.required<LabelElement>();
  /** Gives the field a readable name (e.g. the price list's); raw field id when absent. */
  fieldOption = input<LabelFieldOption | null>(null);
  labelWidth = input.required<number>();
  labelHeight = input.required<number>();

  changed = output<LabelElementPatch>();

  protected readonly typeOptions: TypeOption[] = [
    {
      value: 'text',
      icon: 'pi pi-align-justify',
      labelKey: 'product.labelDesigner.editor.properties.typeText',
    },
    {
      value: 'barcode',
      icon: 'pi pi-barcode',
      labelKey: 'product.labelDesigner.editor.properties.typeBarcode',
    },
  ];

  protected readonly alignmentOptions: AlignmentOption[] = [
    {
      value: 'left',
      icon: 'pi pi-align-left',
      labelKey: 'product.labelDesigner.editor.properties.alignLeft',
    },
    {
      value: 'center',
      icon: 'pi pi-align-center',
      labelKey: 'product.labelDesigner.editor.properties.alignCenter',
    },
    {
      value: 'right',
      icon: 'pi pi-align-right',
      labelKey: 'product.labelDesigner.editor.properties.alignRight',
    },
  ];

  protected readonly form = new FormGroup({
    fontSize: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(MIN_FONT_SIZE_PT),
      Validators.max(MAX_FONT_SIZE_PT),
    ]),
    width: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(MIN_ELEMENT_WIDTH_MM),
    ]),
    height: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(MIN_ELEMENT_HEIGHT_MM),
    ]),
    alignment: new FormControl<LabelElementAlignment>('left', {
      nonNullable: true,
    }),
    type: new FormControl<LabelElementType>('text', { nonNullable: true }),
  });

  constructor() {
    // Mirror the element into the form. Only controls whose value really differs are
    // written, so a half-typed decimal ("2,") is not overwritten by its own echo.
    effect(() => {
      const element = this.element();
      const { fontSize, width, height, alignment } = this.form.controls;
      const next = {
        fontSize: element.fontSize ?? 9,
        width: element.width,
        height: element.height,
        alignment: element.alignment ?? 'left',
      };
      if (height.value !== next.height) {
        height.setValue(next.height, { emitEvent: false });
      }
      if (fontSize.value !== next.fontSize) {
        fontSize.setValue(next.fontSize, { emitEvent: false });
      }
      if (width.value !== next.width) {
        width.setValue(next.width, { emitEvent: false });
      }
      if (alignment.value !== next.alignment) {
        alignment.setValue(next.alignment, { emitEvent: false });
      }
      if (this.form.controls.type.value !== element.type) {
        this.form.controls.type.setValue(element.type, { emitEvent: false });
      }
    });

    this.form.controls.type.valueChanges.pipe(takeUntilDestroyed()).subscribe((type) => {
      const element = this.element();
      if (type !== element.type) {
        this.changed.emit(switchElementType(element, type, this.labelHeight()));
      }
    });

    this.form.controls.fontSize.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.onFontSize(value));

    this.form.controls.width.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.onWidth(value));

    this.form.controls.height.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.onHeight(value));

    this.form.controls.alignment.valueChanges.pipe(takeUntilDestroyed()).subscribe((alignment) => {
      if (alignment && alignment !== this.element().alignment) {
        this.changed.emit({ alignment });
      }
    });
  }

  protected get isBarcode(): boolean {
    return this.element().type === 'barcode';
  }

  /** SKU and barcode fields can be drawn as text or as a barcode. */
  protected get canSwitchType(): boolean {
    return canSwitchToBarcode(this.element().field);
  }

  /** Grows the element's height when the font no longer fits, within the label. */
  private onFontSize(value: number | null): void {
    if (value == null || !Number.isFinite(value) || value < MIN_FONT_SIZE_PT) {
      return;
    }
    const element = this.element();
    const fontSize = round(Math.min(value, MAX_FONT_SIZE_PT));
    if (fontSize === element.fontSize) {
      return;
    }
    const maxHeight = Math.max(this.labelHeight() - element.y, 0);
    const height = Math.min(
      Math.max(element.height, minHeightForFont(fontSize)),
      maxHeight || element.height,
    );
    this.changed.emit(height !== element.height ? { fontSize, height } : { fontSize });
  }

  /** Keeps the element between the minimum width and the label's right edge. */
  private onWidth(value: number | null): void {
    if (value == null || !Number.isFinite(value)) {
      return;
    }
    const element = this.element();
    const maxWidth = Math.max(this.labelWidth() - element.x, MIN_ELEMENT_WIDTH_MM);
    const width = round(Math.min(Math.max(value, MIN_ELEMENT_WIDTH_MM), maxWidth), 2);
    if (width !== element.width) {
      this.changed.emit({ width });
    }
  }

  /** Keeps the element between the minimum height and the label's bottom edge. */
  private onHeight(value: number | null): void {
    if (value == null || !Number.isFinite(value)) {
      return;
    }
    const element = this.element();
    const maxHeight = Math.max(this.labelHeight() - element.y, MIN_ELEMENT_HEIGHT_MM);
    const height = round(Math.min(Math.max(value, MIN_ELEMENT_HEIGHT_MM), maxHeight), 2);
    if (height !== element.height) {
      this.changed.emit({ height });
    }
  }
}
