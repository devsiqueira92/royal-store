import {
  ChangeDetectionStrategy,
  Component,
  Input,
  output,
} from '@angular/core';
import {
  ReactiveFormsModule,
  UntypedFormGroup,
  Validators,
} from '@angular/forms';
import { Options } from '@app/shared/interceptors/options.interface';
import { TranslatePipe } from '@ngx-translate/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { FormErrorComponent } from '../form-error/form-error.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-select-field',
  imports: [
    ReactiveFormsModule,
    SelectModule,
    FloatLabelModule,
    TranslatePipe,
    FormErrorComponent,
  ],
  templateUrl: './select-field.component.html',
  styleUrl: './select-field.component.scss',
})
export class SelectFieldComponent {
  @Input({ required: true }) parentForm!: UntypedFormGroup;
  @Input({ required: true }) controlName!: string;
  @Input() label?: string;
  @Input() placeholder?: string;
  @Input() id?: string;
  @Input() options?: Options[] = [];
  @Input() loading: boolean = false;
  @Input() disabled: boolean = false;
  @Input() showClear: boolean = false;
  @Input() filter: boolean = false;

  onSelect = output<SelectChangeEvent>();

  objectKeys = Object.keys;
  Validators = Validators;
}
