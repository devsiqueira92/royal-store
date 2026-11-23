import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { TranslatePipe } from '@ngx-translate/core';
import { TagModule } from 'primeng/tag';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { LabelFacade } from '../../facade/label.facade';
import {
  LabelDesignerSelectionControls,
  LabelPaperType,
  LabelSheetFormControls,
} from '../../interfaces/label-format.interface';
import {
  LABEL_FIELDS,
  LABEL_SHEET_FIELDS,
} from '../../utils/label-sheet-layout.util';
/** Label name, pre-configuration loader and the sheet/label measurements form. */
@Component({
  selector: 'app-label-format-panel',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    TagModule,
    FormLayoutComponent,
    InputFieldComponent,
    InputNumberFieldComponent,
    SelectFieldComponent,
  ],
  templateUrl: './label-format-panel.component.html',
  styleUrl: './label-format-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelFormatPanelComponent {
  selectionForm = input.required<FormGroup<LabelDesignerSelectionControls>>();
  sheetForm = input.required<FormGroup<LabelSheetFormControls>>();
  paperType = input<LabelPaperType | null>(null);

  private readonly labelFacade = inject(LabelFacade);

  protected readonly labelTypeOptions = this.labelFacade.configurations;
  protected readonly loading = this.labelFacade.isLoadingConfigurations;

  constructor() {
    this.labelFacade.loadConfigurations();
  }

  protected readonly sheetFields = LABEL_SHEET_FIELDS;
  protected readonly labelFields = LABEL_FIELDS;
  protected readonly PaperType = LabelPaperType;
}
