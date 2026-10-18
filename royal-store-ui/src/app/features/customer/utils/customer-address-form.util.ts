import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { Options } from '@app/shared/interceptors/options.interface';
import {
  CustomerAddress,
  CustomerAddressFormControls,
} from '../interfaces/customer-address.interface';
import {
  CustomerAddressChangeRequest,
  CustomerAddressRequest,
} from '../interfaces/customer-address-request.interface';

const BRAZILIAN_STATES = [
  'AC',
  'AL',
  'AP',
  'AM',
  'BA',
  'CE',
  'DF',
  'ES',
  'GO',
  'MA',
  'MT',
  'MS',
  'MG',
  'PA',
  'PB',
  'PR',
  'PE',
  'PI',
  'RJ',
  'RN',
  'RS',
  'RO',
  'RR',
  'SC',
  'SP',
  'SE',
  'TO',
];

export const STATE_OPTIONS: Options[] = BRAZILIAN_STATES.map(state => ({
  code: state,
  description: state,
}));

const ZIP_CODE_PATTERN = /^\d{5}-?\d{3}$/;

const TEXT_FIELDS = [
  'zipCode',
  'street',
  'number',
  'neighborhood',
  'city',
  'state',
  'complement',
  'additionalInfo',
] as const;

export type CustomerAddressFormArray = FormArray<
  FormGroup<CustomerAddressFormControls>
>;

/** Every field is optional on the API; the limits mirror `PostalAddress` there. */
export function buildCustomerAddressGroup(
  address: Partial<CustomerAddress> = {},
): FormGroup<CustomerAddressFormControls> {
  const group = new FormGroup<CustomerAddressFormControls>({
    id: new FormControl(address.id ?? null),
    zipCode: new FormControl(address.zipCode ?? null, [
      Validators.pattern(ZIP_CODE_PATTERN),
    ]),
    street: new FormControl(address.street ?? null, [
      Validators.maxLength(200),
    ]),
    number: new FormControl(address.number ?? null, [Validators.maxLength(20)]),
    neighborhood: new FormControl(address.neighborhood ?? null, [
      Validators.maxLength(100),
    ]),
    city: new FormControl(address.city ?? null, [Validators.maxLength(100)]),
    state: new FormControl(address.state ?? null),
    complement: new FormControl(address.complement ?? null, [
      Validators.maxLength(100),
    ]),
    additionalInfo: new FormControl(address.additionalInfo ?? null, [
      Validators.maxLength(250),
    ]),
    isDefaultDelivery: new FormControl(address.isDefaultDelivery ?? false, {
      nonNullable: true,
    }),
    isActive: new FormControl(address.isActive ?? true, { nonNullable: true }),
  });

  // Inactive addresses are read-only here: the API won't make them the default,
  // and bringing one back is its own reactivate endpoint.
  if (address.isActive === false) {
    group.disable();
  }

  return group;
}

/** Addresses for a new customer: every row is new, so the ids are left out. */
export function toCustomerAddressRequests(
  addresses: CustomerAddressFormArray,
): CustomerAddressRequest[] {
  return rowsToSend(addresses).map(toAddressRequest);
}

/** Address changes for an existing customer; the rows not sent stay as they are. */
export function toCustomerAddressChangeRequests(
  addresses: CustomerAddressFormArray,
): CustomerAddressChangeRequest[] {
  return rowsToSend(addresses).map(group => ({
    id: group.controls.id.value,
    ...toAddressRequest(group),
  }));
}

/** New rows with something typed, and persisted rows the user edited. */
function rowsToSend(
  addresses: CustomerAddressFormArray,
): FormGroup<CustomerAddressFormControls>[] {
  return addresses.controls.filter(group => {
    if (group.disabled) {
      return false;
    }

    return group.controls.id.value ? group.dirty : !isBlank(group);
  });
}

function toAddressRequest(
  group: FormGroup<CustomerAddressFormControls>,
): CustomerAddressRequest {
  const value = group.getRawValue();
  return {
    zipCode: value.zipCode?.replace(/\D/g, '') || null,
    street: clean(value.street),
    number: clean(value.number),
    neighborhood: clean(value.neighborhood),
    city: clean(value.city),
    state: clean(value.state),
    complement: clean(value.complement),
    additionalInfo: clean(value.additionalInfo),
    isDefaultDelivery: value.isDefaultDelivery,
  };
}

function isBlank(group: FormGroup<CustomerAddressFormControls>): boolean {
  return TEXT_FIELDS.every(field => !clean(group.controls[field].value));
}

function clean(value: string | null): string | null {
  return value?.trim() || null;
}
