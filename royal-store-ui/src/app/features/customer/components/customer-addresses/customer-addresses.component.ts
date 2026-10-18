import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import {
  takeUntilDestroyed,
  toObservable,
  toSignal,
} from '@angular/core/rxjs-interop';
import {
  distinctUntilChanged,
  filter,
  map,
  merge,
  shareReplay,
  startWith,
  switchMap,
} from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { CustomerAddressFormControls } from '../../interfaces/customer-address.interface';
import {
  buildCustomerAddressGroup,
  CustomerAddressFormArray,
} from '../../utils/customer-address-form.util';
import { CustomerAddressFormComponent } from '../customer-address-form/customer-address-form.component';

type AddressGroup = FormGroup<CustomerAddressFormControls>;

const sameGroups = (previous: AddressGroup[], current: AddressGroup[]) =>
  previous.length === current.length &&
  previous.every((group, index) => group === current[index]);

@Component({
  selector: 'app-customer-addresses',
  imports: [TranslatePipe, CustomerAddressFormComponent],
  templateUrl: './customer-addresses.component.html',
  styleUrl: './customer-addresses.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerAddressesComponent {
  addresses = input.required<CustomerAddressFormArray>();

  // Zoneless: push/removeAt/clear on a FormArray aren't signals. Every one of
  // them emits valueChanges, so the rows are read from there and only
  // re-emitted when the set of groups itself changes, not on each keystroke.
  private readonly groups$ = toObservable(this.addresses).pipe(
    switchMap(array =>
      array.valueChanges.pipe(
        startWith(null),
        map(() => [...array.controls]),
      ),
    ),
    distinctUntilChanged(sameGroups),
    shareReplay({ bufferSize: 1, refCount: true }),
  );

  protected readonly groups = toSignal(this.groups$, {
    initialValue: [] as AddressGroup[],
  });

  constructor() {
    // A customer has at most one default delivery address: checking one
    // unchecks the rest. The API enforces the same rule on save.
    this.groups$
      .pipe(
        switchMap(groups =>
          merge(
            ...groups.map(group =>
              group.controls.isDefaultDelivery.valueChanges.pipe(
                filter(Boolean),
                map(() => group),
              ),
            ),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe(selected => this.clearOtherDefaults(selected));
  }

  protected addAddress(): void {
    const array = this.addresses();
    // The API makes a customer's first address the default anyway.
    array.push(
      buildCustomerAddressGroup({ isDefaultDelivery: array.length === 0 }),
    );
  }

  protected removeAddress(index: number): void {
    this.addresses().removeAt(index);
  }

  private clearOtherDefaults(selected: AddressGroup): void {
    for (const group of this.addresses().controls) {
      const isDefault = group.controls.isDefaultDelivery;
      if (group !== selected && isDefault.value) {
        isDefault.setValue(false);
      }
    }
  }
}
