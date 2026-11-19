import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { TagModule } from 'primeng/tag';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { Options } from '@app/shared/interceptors/options.interface';
import {
  LabelPaperType,
  LabelSheetFormControls,
} from '../../interfaces/label-format.interface';
import {
  LABEL_FIELDS,
  LABEL_SHEET_FIELDS,
} from '../../utils/label-sheet-layout.util';

/** Presentational: label type dropdown + the sheet/label measurements form. */
@Component({
  selector: 'app-label-format-panel',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    TagModule,
    FormLayoutComponent,
    InputNumberFieldComponent,
    SelectFieldComponent,
  ],
  templateUrl: './label-format-panel.component.html',
  styleUrl: './label-format-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelFormatPanelComponent {
  selectionForm =
    input.required<FormGroup<{ labelTypeId: FormControl<string | null> }>>();
  sheetForm = input.required<FormGroup<LabelSheetFormControls>>();
  labelTypeOptions = input<Options[]>([]);
  paperType = input<LabelPaperType | null>(null);
  loading = input(false);

  protected readonly sheetFields = LABEL_SHEET_FIELDS;
  protected readonly labelFields = LABEL_FIELDS;
  protected readonly PaperType = LabelPaperType;
}
