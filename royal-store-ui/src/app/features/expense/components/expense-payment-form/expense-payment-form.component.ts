import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { CurrencyPipe } from '@angular/common';
import { Options } from '@app/shared/interceptors/options.interface';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { ExpenseFacade } from '../../facade/expense.facade';
import { Expense, FinancialAccount } from '../../interfaces/expense.interface';
import { todayIso } from '../../utils/expense.util';

/**
 * Settles part or all of an expense.
 *
 * The amount defaults to what is still owed, which is the overwhelmingly common
 * case, and is capped at it: the API refuses an overpayment, and finding that
 * out after submitting would lose the shopkeeper's typing.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-payment-form',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    CurrencyPipe,
    ButtonComponent,
    InputFieldComponent,
    InputNumberFieldComponent,
    SelectFieldComponent,
  ],
  templateUrl: './expense-payment-form.component.html',
  styleUrl: './expense-payment-form.component.scss',
})
export class ExpensePaymentFormComponent {
  private readonly expenseFacade = inject(ExpenseFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly expense = toSignal(this.expenseFacade.selectedExpense$, {
    initialValue: null as Expense | null,
  });

  protected readonly isSaving = toSignal(this.expenseFacade.isSaving$, { initialValue: false });

  private readonly accounts = toSignal(this.expenseFacade.financialAccounts$, {
    initialValue: [] as FinancialAccount[],
  });

  protected readonly accountOptions = computed<Options[]>(() =>
    this.accounts()
      .filter((account) => account.isActive)
      .map((account) => ({ code: account.id, description: account.name })),
  );

  protected readonly formGroup = new FormGroup({
    amount: new FormControl<number | null>(null, [Validators.required, Validators.min(0.01)]),
    paidAt: new FormControl<string | null>(todayIso(), [Validators.required]),
    financialAccountId: new FormControl<string | null>(null, [Validators.required]),
    methodNote: new FormControl<string | null>(null, [Validators.maxLength(120)]),
  });

  constructor() {
    effect(() => {
      const expense = this.expense();
      const onlyAccount = this.accountOptions().length === 1 ? this.accountOptions()[0].code : null;

      this.formGroup.reset({
        amount: expense?.remainingBalance ?? null,
        paidAt: todayIso(),
        // With a single account there is nothing to choose, so it is pre-picked.
        financialAccountId: onlyAccount,
        methodNote: null,
      });
    });
  }

  protected onClose(): void {
    this.expenseFacade.clearSelectedExpense();
    this.detailsSidebarService.closeDrawer();
  }

  protected onSubmit(): void {
    const expense = this.expense();
    if (!expense || this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const { amount, paidAt, financialAccountId, methodNote } = this.formGroup.getRawValue();

    this.expenseFacade.registerPayment(expense.id, {
      amount: amount ?? 0,
      paidAt: paidAt!,
      financialAccountId: financialAccountId!,
      methodNote: methodNote?.trim() || null,
      notes: null,
    });

    this.onClose();
  }
}
