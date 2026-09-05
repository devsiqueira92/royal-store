import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
} from '@angular/core';
import {
  ReactiveFormsModule,
  UntypedFormGroup,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import { MessageModule } from 'primeng/message';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { InputTextModule } from 'primeng/inputtext';
import { FormErrorComponent } from '../form-error/form-error.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-input-number-field',
  imports: [
    ReactiveFormsModule,
    MessageModule,
    FloatLabelModule,
    TranslatePipe,
    InputNumberModule,
    InputGroupModule,
    InputGroupAddonModule,
    InputTextModule,
    FormErrorComponent,
  ],
  templateUrl: './input-number-field.component.html',
  styleUrl: './input-number-field.component.scss',
})
export class InputNumberFieldComponent {
  parentForm = input.required<UntypedFormGroup>();
  controlName = input.required<string>();
  label = input<string>();
  id = input<string>();
  mode = input<string>('currency');
  min = input<number>();
  step = input<number>();
  prefix = input<string>('R$');

  blurred = output<void>();

  objectKeys = Object.keys;
  Validators = Validators;

  formatCurrency(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    if (!inputElement) return;

    const numericString = inputElement.value.replace(/\D/g, '');
    const cents = parseInt(numericString || '0', 10);
    const floatValue = cents / 100;
    this.parentForm().controls[this.controlName()].setValue(floatValue, {
      emitEvent: false,
    });
    inputElement.value = floatValue.toFixed(2);
  }
}
