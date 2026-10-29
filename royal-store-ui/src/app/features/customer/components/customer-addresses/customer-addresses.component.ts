import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
} from '@angular/core';
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
  Observable,
  shareReplay,
  startWith,
  switchMap,
} from 'rxjs';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { CustomerAddressFormControls } from '../../interfaces/customer-address.interface';
import {
  buildCustomerAddressGroup,
  CustomerAddressFormArray,
} from '../../utils/customer-address-form.util';
import { CustomerAddressFormComponent } from '../customer-address-form/customer-address-form.component';
import { ViaCepService } from '@app/shared/services/via-cep.service';
import { NotificationService } from '@app/shared/services/notification.service';
import { ViaCepAddress } from '@app/shared/interfaces/via-cep.interface';

type AddressGroup = FormGroup<CustomerAddressFormControls>;

interface ZipCodeLookup {
  group: AddressGroup;
  address: ViaCepAddress | null;
}

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
  private readonly viaCepService = inject(ViaCepService);
  private readonly notificationService = inject(NotificationService);
  private readonly translateService = inject(TranslateService);

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

    this.groups$
      .pipe(
        switchMap(groups =>
          merge(...groups.map(group => this.lookupZipCode(group))),
        ),
        takeUntilDestroyed(),
      )
      .subscribe(lookup => this.applyZipCodeLookup(lookup));
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

  private lookupZipCode(group: AddressGroup): Observable<ZipCodeLookup> {
    return this.viaCepService
      .lookupChanges(group.controls.zipCode)
      .pipe(map(address => ({ group, address })));
  }

  private applyZipCodeLookup({ group, address }: ZipCodeLookup): void {
    if (!address) {
      this.notificationService.warn({
        detail: this.translateService.instant(
          'customer.address.zipCodeNotFound',
        ),
      });
      return;
    }

    // Zip codes that cover a whole town come back without street or
    // neighborhood: keep what the user already typed there. ViaCEP's
    // `complemento` describes the zip code's range ("lado ímpar"), not the
    // customer's unit, so it is left alone.
    const { street, neighborhood, city, state } = group.controls;
    group.patchValue({
      street: address.logradouro || street.value,
      neighborhood: address.bairro || neighborhood.value,
      city: address.localidade || city.value,
      state: address.uf || state.value,
    });
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
