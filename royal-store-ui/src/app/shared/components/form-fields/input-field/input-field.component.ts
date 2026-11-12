import { ChangeDetectionStrategy, Component, Input, output } from '@angular/core';
import { ReactiveFormsModule, UntypedFormGroup, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { FormErrorComponent } from '../form-error/form-error.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-input-field',
  imports: [
    ReactiveFormsModule,
    InputTextModule,
    MessageModule,
    FloatLabelModule,
    TranslatePipe,
    FormErrorComponent,
  ],
  templateUrl: './input-field.component.html',
  styleUrl: './input-field.component.scss',
})
export class InputFieldComponent {
  @Input({ required: true }) parentForm!: UntypedFormGroup;
  @Input({ required: true }) controlName!: string;
  @Input() label?: string;
  @Input() placeholder?: string;
  @Input() id?: string;
  @Input() type: string = 'text';
  @Input() min?: number;
  @Input() step?: number;
  /**
   * Defaults to `off` for the app's internal forms (product, customer, …),
   * where browser suggestions are noise. Sign-in and sign-up screens pass a
   * real token (`email`, `username`, …) so password managers and autofill can
   * do their job — blanket `autocomplete="off"` is the documented anti-pattern.
   */
  @Input() autocomplete: string = 'off';
  /** `numeric`/`tel`/`email` brings up the right on-screen keyboard. */
  @Input() inputmode?: string;

  blurred = output<void>();

  objectKeys = Object.keys;
  Validators = Validators;
}
