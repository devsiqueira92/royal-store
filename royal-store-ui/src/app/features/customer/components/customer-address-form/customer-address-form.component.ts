import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
} from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { map, startWith, switchMap } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { CheckboxModule } from 'primeng/checkbox';
import { CustomerAddressFormControls } from '../../interfaces/customer-address.interface';
import { STATE_OPTIONS } from '../../utils/customer-address-form.util';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { TextareaFieldComponent } from '@app/shared/components/form-fields/textarea-field/textarea-field.component';

@Component({
  selector: 'app-customer-address-form',
  imports: [
    ReactiveFormsModule,
    DecimalPipe,
    TranslatePipe,
    CheckboxModule,
    InputFieldComponent,
    SelectFieldComponent,
    TextareaFieldComponent,
  ],
  templateUrl: './customer-address-form.component.html',
  styleUrl: './customer-address-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerAddressFormComponent {
  addressGroup = input.required<FormGroup<CustomerAddressFormControls>>();
  index = input.required<number>();

  remove = output<void>();

  protected readonly stateOptions = STATE_OPTIONS;

  private readonly values = toSignal(
    toObservable(this.addressGroup).pipe(
      switchMap(group =>
        group.valueChanges.pipe(
          startWith(null),
          map(() => group.getRawValue()),
        ),
      ),
    ),
  );

  protected readonly summary = computed(() => {
    const values = this.values();
    if (!values) {
      return '';
    }

    const street = [values.street, values.number].filter(Boolean).join(', ');
    const place = [values.city, values.state].filter(Boolean).join(' - ');
    return [street, place].filter(Boolean).join(' · ');
  });

  // Persisted addresses can't be deleted, only deactivated on the API.
  protected readonly isNew = computed(() => !this.values()?.id);

  protected readonly isDefault = computed(
    () => !!this.values()?.isDefaultDelivery,
  );

  protected readonly isInactive = computed(
    () => this.values()?.isActive === false,
  );
}
