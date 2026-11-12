import { ChangeDetectionStrategy, Component, input, Input } from '@angular/core';
import { ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ToggleButtonModule } from 'primeng/togglebutton';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-toggle-field',
  imports: [TranslatePipe, ToggleButtonModule, ReactiveFormsModule],
  templateUrl: './toggle-field.component.html',
  styleUrl: './toggle-field.component.scss',
})
export class ToggleFieldComponent {
  @Input({ required: true }) parentForm!: UntypedFormGroup;
  @Input({ required: true }) controlName!: string;
  @Input() titleKey?: string;
  @Input() subtitleKey?: string;
  disabled = input<boolean>(false);

  protected toggle(): void {
    const control = this.parentForm.controls[this.controlName];
    control.setValue(!control.value);
  }
}
