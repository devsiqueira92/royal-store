import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { filter } from 'rxjs';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { Options } from '@app/shared/interceptors/options.interface';
import { ExpenseFacade } from '../../facade/expense.facade';
import { FinancialAccount, FinancialAccountType } from '../../interfaces/expense.interface';
import { FinancialAccountFormControls } from '../../interfaces/expense-request.interface';

const DEFAULT_ACCOUNT_TYPE: FinancialAccountType = 'bank-account';

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
    // The account being edited; none while creating (the list clears it before opening the drawer).
    this.expenseFacade.selectedAccount$
      .pipe(
        filter(
          (account): account is FinancialAccount =>
            !!account && account.id !== this.formGroup.controls.id.value,
        ),
        takeUntilDestroyed(),
      )
      .subscribe((account) => this.populateForm(account));

    // Closed only once the API accepted the change; on failure the drawer stays open with the input.
    this.expenseFacade.accountSaved$
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.onClose());
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

    const { id, name, type } = this.formGroup.getRawValue();
    const request = { name: (name ?? '').trim(), type };

    if (id) {
      this.expenseFacade.updateAccount(id, request);
    } else {
      this.expenseFacade.createAccount(request);
    }
  }

  private populateForm(account: FinancialAccount): void {
    this.formGroup.reset({ id: account.id, name: account.name, type: account.type });
  }

  private buildFormGroup(): FormGroup<FinancialAccountFormControls> {
    return new FormGroup<FinancialAccountFormControls>({
      id: new FormControl(null),
      name: new FormControl(null, [Validators.required, Validators.maxLength(100)]),
      type: new FormControl<FinancialAccountType>(DEFAULT_ACCOUNT_TYPE, {
        nonNullable: true,
        validators: [Validators.required],
      }),
    });
  }
}
