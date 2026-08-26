import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-toggle-field',
  imports: [TranslatePipe],
  templateUrl: './toggle-field.component.html',
  styleUrl: './toggle-field.component.scss',
})
export class ToggleFieldComponent {
  @Input({ required: true }) parentForm!: UntypedFormGroup;
  @Input({ required: true }) controlName!: string;
  @Input() titleKey?: string;
  @Input() subtitleKey?: string;

  protected toggle(): void {
    const control = this.parentForm.controls[this.controlName];
    control.setValue(!control.value);
  }
}
