import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { Options } from '@app/shared/interceptors/options.interface';
import { ExpenseFacade } from '../../facade/expense.facade';
import { FinancialAccount, FinancialAccountType } from '../../interfaces/expense.interface';
import { FinancialAccountFormControls } from '../../interfaces/expense-request.interface';

/**
 * The New / Edit financial account drawer.
 *
 * Two fields and no balance: an account answers "where did the money leave
 * from?", and what left is derived from the payments themselves (FR-026a).
 *
 * Active is not part of the submit — it has its own endpoint and is toggled
 * from the list, so a rename cannot silently reactivate an account.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-account-form',
  imports: [
    ReactiveFormsModule,
    FormLayoutComponent,
    InputFieldComponent,
    SelectFieldComponent,
    ButtonComponent,
  ],
  templateUrl: './expense-account-form.component.html',
  styleUrl: './expense-account-form.component.scss',
})
export class ExpenseAccountFormComponent {
  private readonly expenseFacade = inject(ExpenseFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);
  private readonly translateService = inject(TranslateService);

  protected readonly isCreateMode = computed(
    () => this.detailsSidebarService.contentToRender()?.props?.mode === 'create',
  );

  protected readonly selectedAccount = toSignal(this.expenseFacade.selectedAccount$, {
    initialValue: null as FinancialAccount | null,
  });

  protected readonly isSaving = toSignal(this.expenseFacade.isSaving$, { initialValue: false });

  /**
   * The three types the API accepts, by the very names it emits — it parses
   * `cash` / `bank-account` / `digital-wallet` and rejects anything else with a
   * 400 before validation can explain why.
   */
  protected readonly typeOptions: Options[] = (
    ['cash', 'bank-account', 'digital-wallet'] as const
  ).map((type) => ({
    code: type,
    description: this.translateService.instant(`expense.accounts.types.${type}`),
  }));

  protected readonly formGroup = this.buildFormGroup();

  constructor() {
    effect(() => {
      if (this.isCreateMode()) {
        this.enterCreateMode();
        return;
      }

      this.populateForm(this.selectedAccount());
    });
  }

  protected onClose(): void {
    this.expenseFacade.clearSelectedAccount();
    this.detailsSidebarService.closeDrawer();
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const { name, type } = this.formGroup.getRawValue();
    const request = { name: (name ?? '').trim(), type };

    if (this.isCreateMode()) {
      this.expenseFacade.createAccount(request);
    } else {
      const current = this.selectedAccount();
      if (!current) {
        return;
      }

      this.expenseFacade.updateAccount(current.id, request);
    }

    this.onClose();
  }

  private enterCreateMode(): void {
    this.formGroup.reset({ name: null, type: 'bank-account' });
  }

  private populateForm(account: FinancialAccount | null): void {
    this.formGroup.reset({
      name: account?.name ?? null,
      type: account?.type ?? 'bank-account',
    });
  }

  private buildFormGroup(): FormGroup<FinancialAccountFormControls> {
    return new FormGroup<FinancialAccountFormControls>({
      name: new FormControl(null, [Validators.required, Validators.maxLength(100)]),
      type: new FormControl<FinancialAccountType>('bank-account', {
        nonNullable: true,
        validators: [Validators.required],
      }),
    });
  }
}
