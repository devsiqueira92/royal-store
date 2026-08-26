import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import {
  ReactiveFormsModule,
  UntypedFormGroup,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import { MessageModule } from 'primeng/message';
import { TextareaModule } from 'primeng/textarea';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-textarea-field',
  imports: [
    ReactiveFormsModule,
    TextareaModule,
    MessageModule,
    FloatLabelModule,
    TranslatePipe,
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
