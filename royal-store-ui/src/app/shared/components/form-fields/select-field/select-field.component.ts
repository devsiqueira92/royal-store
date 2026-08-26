import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import {
  ReactiveFormsModule,
  UntypedFormGroup,
  Validators,
} from '@angular/forms';
import { Options } from '@app/shared/interceptors/options.interface';
import { TranslatePipe } from '@ngx-translate/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-select-field',
  imports: [
    ReactiveFormsModule,
    SelectModule,
    MessageModule,
    FloatLabelModule,
    TranslatePipe,
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

  objectKeys = Object.keys;
  Validators = Validators;
}
