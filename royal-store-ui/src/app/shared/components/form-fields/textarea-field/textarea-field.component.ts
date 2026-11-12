import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ReactiveFormsModule, UntypedFormGroup, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import { TextareaModule } from 'primeng/textarea';
import { FormErrorComponent } from '../form-error/form-error.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-textarea-field',
  imports: [
    ReactiveFormsModule,
    TextareaModule,
    FloatLabelModule,
    TranslatePipe,
    FormErrorComponent,
  ],
  templateUrl: './textarea-field.component.html',
  styleUrl: './textarea-field.component.scss',
})
export class TextareaFieldComponent {
  @Input({ required: true }) parentForm!: UntypedFormGroup;
  @Input({ required: true }) controlName!: string;
  @Input() label?: string;
  @Input() placeholder?: string;
  @Input() id?: string;
  @Input() rows: number = 3;

  objectKeys = Object.keys;
  Validators = Validators;
}
