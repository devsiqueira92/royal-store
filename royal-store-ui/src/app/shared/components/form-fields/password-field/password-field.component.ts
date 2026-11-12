import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ReactiveFormsModule, UntypedFormGroup, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { PasswordModule } from 'primeng/password';
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-password-field',
  imports: [
    ReactiveFormsModule,
    InputTextModule,
    MessageModule,
    FloatLabelModule,
    PasswordModule,
    TranslatePipe,
  ],
  templateUrl: './password-field.component.html',
  styleUrl: './password-field.component.scss',
})
export class PasswordFieldComponent {
  @Input({ required: true }) parentForm!: UntypedFormGroup;
  @Input({ required: true }) controlName!: string;
  @Input() label?: string;
  @Input() id?: string;
  /**
   * `current-password` on sign-in, `new-password` on register/reset. Password
   * managers rely on this to offer the right credential and to save a new one,
   * which WCAG 2.2 Accessible Authentication expects us not to block.
   */
  @Input() autocomplete: 'current-password' | 'new-password' = 'current-password';

  objectKeys = Object.keys;
  Validators = Validators;
}
