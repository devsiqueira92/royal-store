import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import {
  ReactiveFormsModule,
  UntypedFormGroup,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-input-field',
  imports: [
    ReactiveFormsModule,
    InputTextModule,
    MessageModule,
    FloatLabelModule,
    TranslatePipe,
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

  objectKeys = Object.keys;
  Validators = Validators;
}
