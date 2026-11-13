import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { SelectButtonModule } from 'primeng/selectbutton';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { PlainDatePipe } from '@app/shared/pipes/plain-date.pipe';
import { ExpenseFacade } from '../../facade/expense.facade';
import {
  Expense,
  ExpenseCategorySummary,
} from '../../interfaces/expense.interface';
import { ExpenseFormControls } from '../../interfaces/expense-request.interface';
import { ExpenseCategoryPickerComponent } from '../expense-category-picker/expense-category-picker.component';
import {
  dayOf,
  firstDayOfMonth,
  shiftCompetenceMonth,
  todayIso,
} from '../../utils/expense.util';

/**
 * The New / Edit expense drawer.
 *
 * Two things are recorded from one form. With "repeat monthly" off it records a
 * single bill; with it on it registers a recurring TEMPLATE and the generator
 * creates each month's bill from it. They are different resources on the API,
 * which is why the three fields under the toggle only exist in that branch —
 * a lead time and a variable amount mean nothing to a one-off expense.
 *
 * Editing never offers the recurrence branch: a generated bill is an expense,
 * and changing its template from here would silently rewrite future months.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    SelectButtonModule,
    ButtonComponent,
    SwitchComponent,
    InputFieldComponent,
    InputNumberFieldComponent,
    ExpenseCategoryPickerComponent,
    PlainDatePipe,
  ],
  templateUrl: './expense-form.component.html',
  styleUrl: './expense-form.component.scss',
})
export class ExpenseFormComponent {
  private readonly expenseFacade = inject(ExpenseFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly isCreateMode = computed(
    () =>
      this.detailsSidebarService.contentToRender()?.props?.mode === 'create',
  );

  protected readonly selectedExpense = toSignal(
    this.expenseFacade.selectedExpense$,
    {
      initialValue: null as Expense | null,
    },
  );

  protected readonly categories = toSignal(this.expenseFacade.categories$, {
    initialValue: [] as ExpenseCategorySummary[],
  });

  protected readonly isSaving = toSignal(this.expenseFacade.isSaving$, {
    initialValue: false,
  });

  /** How many days before the due date a generated bill is created. */
  protected readonly leadDayOptions = [5, 10, 15];

  protected readonly formGroup = this.buildFormGroup();

  /**
   * The form's value as a signal. A reactive form is not reactive to signals on
   * its own, so everything the template derives from it — the revealed section,
   * the picked category, the summary line — reads this instead of the controls.
   */
  private readonly formValue = toSignal(this.formGroup.valueChanges, {
    initialValue: this.formGroup.getRawValue(),
  });

  protected readonly repeatMonthly = computed(
    () => this.formValue().repeatMonthly ?? false,
  );
  protected readonly isVariableAmount = computed(
    () => this.formValue().isVariableAmount ?? false,
  );
  protected readonly selectedCategoryId = computed(
    () => this.formValue().expenseCategoryId ?? null,
  );
  protected readonly dueDate = computed(() => this.formValue().dueDate ?? '');

  /** The picker is not an input, so its "touched" state is tracked here. */
  private readonly categoryTouched = signal(false);

  protected readonly categoryInvalid = computed(
    () => this.categoryTouched() && !this.selectedCategoryId(),
  );

  constructor() {
    effect(() => {
      if (this.isCreateMode()) {
        this.enterCreateMode();
        return;
      }

      this.populateForm(this.selectedExpense());
    });
  }

  protected onCategorySelected(id: string): void {
    this.formGroup.controls.expenseCategoryId.setValue(id);
    this.formGroup.controls.expenseCategoryId.markAsTouched();
    this.categoryTouched.set(true);
  }

  protected onRepeatChange(repeat: boolean): void {
    this.formGroup.controls.repeatMonthly.setValue(repeat);
  }

  protected onVariableAmountChange(variable: boolean): void {
    this.formGroup.controls.isVariableAmount.setValue(variable);
  }

  protected onClose(): void {
    this.expenseFacade.clearSelectedExpense();
    this.detailsSidebarService.closeDrawer();
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.categoryTouched.set(true);
      return;
    }

    const { description, payeeName, expenseCategoryId, amount, dueDate } =
      this.formGroup.getRawValue();

    // Shared by both branches. `notes` is NOT here: a recurrence template has no
    // notes field, and sending one made the payload carry a property the API
    // silently drops.
    const common = {
      description: (description ?? '').trim(),
      payeeName: (payeeName ?? '').trim(),
      supplierId: null,
      expenseCategoryId: expenseCategoryId!,
      costCenterId: null,
      financialAccountId: null,
      amount: amount ?? 0,
    };

    if (this.isCreateMode() && this.formGroup.controls.repeatMonthly.value) {
      this.expenseFacade.createRecurrence({
        ...common,
        frequency: 'monthly',
        dueDay: dayOf(dueDate!),
        leadDays: this.formGroup.controls.leadDays.value,
        isVariableAmount: this.formGroup.controls.isVariableAmount.value,
        startDate: dueDate!,
        endDate: null,
        maxOccurrences: null,
      });

      this.onClose();
      return;
    }

    const expense = {
      ...common,
      // The competence month follows the due date: a bill due in December is
      // December's cost, whichever month it was typed in.
      competenceMonth: firstDayOfMonth(dueDate!.slice(0, 7)),
      dueDate: dueDate!,
    };

    if (this.isCreateMode()) {
      this.expenseFacade.createExpense({ ...expense, isEstimated: false });
    } else {
      const current = this.selectedExpense();
      if (!current) {
        return;
      }

      this.expenseFacade.updateExpense(current.id, expense);
    }

    this.onClose();
  }

  /** "next bill Dec 28" — the cycle after the start date, for the summary line. */
  protected readonly nextBillDate = computed(() => {
    const dueDate = this.dueDate() || todayIso();
    const today = todayIso();

    return dueDate > today
      ? dueDate
      : `${shiftCompetenceMonth(dueDate.slice(0, 7), 1)}-${dueDate.slice(8)}`;
  });

  private enterCreateMode(): void {
    this.formGroup.reset({
      description: null,
      payeeName: null,
      expenseCategoryId: null,
      amount: null,
      // Month end is the common due date for the bills this module is for
      // (rent, utilities, payroll), and it is always a valid date.
      dueDate: this.lastDayOfCurrentMonth(),
      repeatMonthly: false,
      isVariableAmount: false,
      leadDays: 10,
    });
  }

  private populateForm(expense: Expense | null): void {
    this.formGroup.reset({
      description: expense?.description ?? null,
      payeeName: expense?.payeeName ?? null,
      expenseCategoryId: expense?.category.id ?? null,
      amount: expense?.amount ?? null,
      dueDate: expense?.dueDate ?? this.lastDayOfCurrentMonth(),
      repeatMonthly: false,
      isVariableAmount: false,
      leadDays: 10,
    });
  }

  /** `yyyy-MM-dd`; day 0 of next month is the last day of this one. */
  private lastDayOfCurrentMonth(): string {
    const now = new Date();
    const last = new Date(now.getFullYear(), now.getMonth() + 1, 0);

    return `${last.getFullYear()}-${`${last.getMonth() + 1}`.padStart(2, '0')}-${`${last.getDate()}`.padStart(2, '0')}`;
  }

  private buildFormGroup(): FormGroup<ExpenseFormControls> {
    return new FormGroup<ExpenseFormControls>({
      description: new FormControl(null, [
        Validators.required,
        Validators.maxLength(200),
      ]),
      payeeName: new FormControl(null, [Validators.maxLength(200)]),
      expenseCategoryId: new FormControl(null, [Validators.required]),
      amount: new FormControl(null, [
        Validators.required,
        Validators.min(0.01),
      ]),
      dueDate: new FormControl(null, [Validators.required]),
      repeatMonthly: new FormControl(false, { nonNullable: true }),
      isVariableAmount: new FormControl(false, { nonNullable: true }),
      leadDays: new FormControl(10, { nonNullable: true }),
    });
  }
}
