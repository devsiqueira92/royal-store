import {
  ChangeDetectionStrategy,
  Component,
  input,
  Input,
  output,
} from '@angular/core';
import {
  ReactiveFormsModule,
  UntypedFormGroup,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { FormErrorComponent } from '../form-error/form-error.component';
import { DatePickerModule } from 'primeng/datepicker';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-datepicker-field',
  imports: [
    ReactiveFormsModule,
    InputTextModule,
    MessageModule,
    FloatLabelModule,
    TranslatePipe,
    FormErrorComponent,
    DatePickerModule,
  ],
  templateUrl: './datepicker-field.component.html',
  styleUrl: './datepicker-field.component.scss',
})
export class DatepickerFieldComponent {
  parentForm = input.required<UntypedFormGroup>();
  controlName = input.required<string>();
  label = input.required<string>();
  placeholder? = input<string>();
  id? = input<string>();
  maxDate = input<Date | null>();
  minDate = input<Date | null>();
  showWeek = input<boolean>(false);
  inline = input<boolean>(false);

  objectKeys = Object.keys;
  Validators = Validators;
}
