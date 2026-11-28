import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
} from '@angular/core';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { combineLatest, filter, map, take, tap } from 'rxjs';
import { Options } from '@app/shared/interceptors/options.interface';
import { CustomerFormControls } from '../../interfaces/customer.interface';
import { CashbackEntry } from '../../interfaces/cashback-entry.interface';
import {
  CustomerAddress,
  CustomerAddressFormControls,
} from '../../interfaces/customer-address.interface';
import {
  buildCustomerAddressGroup,
  toCustomerAddressChangeRequests,
  toCustomerAddressRequests,
} from '../../utils/customer-address-form.util';
import { CustomerFacade } from '../../facade/customer.facade';
import { LoyaltyTierFacade } from '@app/core/loyalty-tier/facade/loyalty-tier.facade';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { CustomerCashbackHistoryComponent } from '../customer-cashback-history/customer-cashback-history.component';
import { CustomerAddressesComponent } from '../customer-addresses/customer-addresses.component';
import { AsyncPipe } from '@angular/common';
import { DatepickerFieldComponent } from '@app/shared/components/form-fields/datepicker-field/datepicker-field.component';
import { formatDateOnly, parseDateOnly } from '@app/shared/utils/date-only.util';

@Component({
  imports: [
    ReactiveFormsModule,
    InputFieldComponent,
    SelectFieldComponent,
    FormLayoutComponent,
    ButtonComponent,
    CustomerAddressesComponent,
    CustomerCashbackHistoryComponent,
    AsyncPipe,
    DatepickerFieldComponent,
  ],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerFormComponent {
  private readonly customerFacade = inject(CustomerFacade);
  private readonly loyaltyTierFacade = inject(LoyaltyTierFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly selectedCustomer$ =
    this.customerFacade.selectedCustomer$.pipe(
      filter(customer => !!customer),
      take(1),
      tap(customer => {
        this.formGroup.patchValue({
          ...customer,
          birthDate: parseDateOnly(customer?.birthDate),
        });
        this.setAddresses(customer?.addresses ?? []);
      }),
    );

  protected readonly isCreateMode = computed(
    () =>
      this.detailsSidebarService.contentToRender()?.props?.mode === 'create',
  );

  protected readonly cashbackHistory = toSignal(
    this.customerFacade.cashbackHistory$,
    { initialValue: [] as CashbackEntry[] },
  );

  protected readonly isLoadingHistory = toSignal(
    this.customerFacade.isLoadingHistory$,
    { initialValue: false },
  );

  /**
   * Active tiers, plus the edited customer's own tier when it has since been deactivated — so the
   * dropdown still shows (and keeps) what the customer has.
   */
  protected readonly tierOptions = toSignal(
    combineLatest([
      this.loyaltyTierFacade.loyaltyTierOptions$,
      this.customerFacade.selectedCustomer$,
    ]).pipe(
      map(([options, customer]) =>
        customer && !options.some(o => o.code === customer.loyaltyTierId)
          ? [
              ...options,
              {
                code: customer.loyaltyTierId,
                description: customer.loyaltyTierName,
              },
            ]
          : options,
      ),
    ),
    { initialValue: [] as Options[] },
  );

  protected readonly formGroup = this.buildFormGroup();

  constructor() {
    this.loyaltyTierFacade.loadLoyaltyTierOptions();

    this.customerFacade.saved$
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.onClose());
  }

  protected onClose(): void {
    this.customerFacade.clearSelectedCustomer();
    this.detailsSidebarService.closeDrawer();
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const { id, name, document, phone, birthDate, loyaltyTierId } =
      this.formGroup.getRawValue();

    const addresses = this.formGroup.controls.addresses;

    if (id) {
      this.customerFacade.updateCustomer(id, {
        name: (name ?? '').trim(),
        phone: phone?.trim() || null,
        birthDate: formatDateOnly(birthDate),
        loyaltyTierId,
        addresses: toCustomerAddressChangeRequests(addresses),
      });
    } else {
      this.customerFacade.createCustomer({
        name: (name ?? '').trim(),
        document: document?.trim() || null,
        phone: phone?.trim() || null,
        birthDate: formatDateOnly(birthDate),
        loyaltyTierId,
        addresses: toCustomerAddressRequests(addresses),
      });
    }
  }

  private setAddresses(addresses: CustomerAddress[]): void {
    const array = this.formGroup.controls.addresses;
    array.clear({ emitEvent: false });
    for (const address of addresses) {
      array.push(buildCustomerAddressGroup(address), { emitEvent: false });
    }
    array.updateValueAndValidity();
  }

  private buildFormGroup(): FormGroup<CustomerFormControls> {
    return new FormGroup<CustomerFormControls>({
      id: new FormControl(null),
      name: new FormControl(null, [
        Validators.required,
        Validators.maxLength(150),
      ]),
      document: new FormControl(null),
      phone: new FormControl(null, [Validators.maxLength(20)]),
      birthDate: new FormControl(null),
      loyaltyTierId: new FormControl(null),
      addresses: new FormArray<FormGroup<CustomerAddressFormControls>>([]),
    });
  }
}
