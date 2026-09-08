import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import {
  Customer,
  CustomerFormControls,
} from '../../interfaces/customer.interface';
import { CashbackEntry } from '../../interfaces/cashback-entry.interface';
import { CustomerFacade } from '../../facade/customer.facade';
import { LoyaltyTierFacade } from '@app/core/loyalty-tier/facade/loyalty-tier.facade';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { CustomerCashbackHistoryComponent } from '../customer-cashback-history/customer-cashback-history.component';

@Component({
  selector: 'app-customer-form',
  imports: [
    ReactiveFormsModule,
    InputFieldComponent,
    SelectFieldComponent,
    FormLayoutComponent,
    ButtonComponent,
    CustomerCashbackHistoryComponent,
  ],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerFormComponent {
  private readonly customerFacade = inject(CustomerFacade);
  private readonly loyaltyTierFacade = inject(LoyaltyTierFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly isCreateMode = computed(
    () =>
      this.detailsSidebarService.contentToRender()?.props?.mode === 'create',
  );

  protected readonly selectedCustomer = toSignal(
    this.customerFacade.selectedCustomer$,
    { initialValue: null as Customer | null },
  );

  protected readonly cashbackHistory = toSignal(
    this.customerFacade.cashbackHistory$,
    { initialValue: [] as CashbackEntry[] },
  );

  protected readonly isLoadingHistory = toSignal(
    this.customerFacade.isLoadingHistory$,
    { initialValue: false },
  );

  protected readonly tierOptions = toSignal(
    this.loyaltyTierFacade.loyaltyTiers$.pipe(
      map(tiers =>
        tiers.map(tier => ({ code: tier.id, description: tier.name })),
      ),
    ),
    { initialValue: [] },
  );

  protected readonly formGroup = this.buildFormGroup();

  constructor() {
    this.loyaltyTierFacade.loadLoyaltyTiers();

    effect(() => {
      if (this.isCreateMode()) {
        this.enterCreateMode();
        return;
      }

      this.populateForm(this.selectedCustomer());
    });
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

    const { name, document, phone, loyaltyTierId } =
      this.formGroup.getRawValue();

    if (this.isCreateMode()) {
      this.customerFacade.createCustomer({
        name: (name ?? '').trim(),
        document: (document ?? '').trim(),
        phone: phone?.trim() || null,
        loyaltyTierId,
      });
    } else {
      const customer = this.selectedCustomer();
      if (!customer) {
        return;
      }

      this.customerFacade.updateCustomer(customer.id, {
        name: (name ?? '').trim(),
        phone: phone?.trim() || null,
        loyaltyTierId,
      });
    }

    this.onClose();
  }

  private enterCreateMode(): void {
    this.formGroup.controls.document.enable();
    this.formGroup.reset({
      id: null,
      name: null,
      document: null,
      phone: null,
      loyaltyTierId: null,
    });
  }

  private populateForm(customer: Customer | null): void {
    // The API has no endpoint to change a document once the customer exists.
    this.formGroup.controls.document.disable();
    this.formGroup.reset({
      id: customer?.id ?? null,
      name: customer?.name ?? null,
      document: customer?.document ?? null,
      phone: customer?.phone ?? null,
      loyaltyTierId: customer?.loyaltyTierId ?? null,
    });
  }

  private buildFormGroup(): FormGroup<CustomerFormControls> {
    return new FormGroup<CustomerFormControls>({
      id: new FormControl(null),
      name: new FormControl(null, [
        Validators.required,
        Validators.maxLength(150),
      ]),
      document: new FormControl(null, [Validators.required]),
      phone: new FormControl(null, [Validators.maxLength(20)]),
      loyaltyTierId: new FormControl(null),
    });
  }
}
