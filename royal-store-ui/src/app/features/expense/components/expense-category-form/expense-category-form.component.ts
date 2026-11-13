import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { Options } from '@app/shared/interceptors/options.interface';
import { ExpenseFacade } from '../../facade/expense.facade';
import { EXPENSE_CATEGORY_COLORS } from '../../constants/expense-category-colors.constant';
import { ExpenseCategory, ExpenseCategoryRow } from '../../interfaces/expense.interface';
import { ExpenseCategoryFormControls } from '../../interfaces/expense-request.interface';

/**
 * The New / Edit category drawer.
 *
 * Active is NOT part of the submit: it has its own endpoint and is toggled
 * straight from the list, so a rename cannot silently reactivate a category.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-category-form',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    FormLayoutComponent,
    InputFieldComponent,
    SelectFieldComponent,
    ButtonComponent,
  ],
  templateUrl: './expense-category-form.component.html',
  styleUrl: './expense-category-form.component.scss',
})
export class ExpenseCategoryFormComponent {
  private readonly expenseFacade = inject(ExpenseFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly isCreateMode = computed(
    () => this.detailsSidebarService.contentToRender()?.props?.mode === 'create',
  );

  protected readonly selectedCategory = toSignal(this.expenseFacade.selectedCategory$, {
    initialValue: null as ExpenseCategory | null,
  });

  protected readonly isSaving = toSignal(this.expenseFacade.isSaving$, { initialValue: false });

  private readonly rows = toSignal(this.expenseFacade.categoryRows$, {
    initialValue: [] as ExpenseCategoryRow[],
  });

  protected readonly colors = EXPENSE_CATEGORY_COLORS;

  protected readonly formGroup = this.buildFormGroup();

  private readonly formValue = toSignal(this.formGroup.valueChanges, {
    initialValue: this.formGroup.getRawValue(),
  });

  protected readonly selectedColor = computed(() => this.formValue().color ?? null);

  /**
   * What can be a parent. The category itself and everything beneath it are
   * left out: the API refuses a move that would close a loop, and offering it
   * here would only produce a refusal.
   */
  protected readonly parentOptions = computed<Options[]>(() => {
    const current = this.selectedCategory();
    const editing = !this.isCreateMode() && current;
    const currentRow = editing ? this.rows().find((row) => row.id === current.id) : undefined;
    const subtreePrefix = currentRow ? `${currentRow.path} › ` : null;

    return this.rows()
      .filter((row) => !editing || row.id !== current.id)
      .filter((row) => !subtreePrefix || !row.path.startsWith(subtreePrefix))
      .map((row) => ({ code: row.id, description: row.path }));
  });

  constructor() {
    effect(() => {
      if (this.isCreateMode()) {
        this.enterCreateMode();
        return;
      }

      this.populateForm(this.selectedCategory());
    });
  }

  /** Clicking the chosen swatch again clears the colour — the API accepts null. */
  protected onColorSelected(color: string): void {
    const control = this.formGroup.controls.color;

    control.setValue(control.value === color ? null : color);
    control.markAsDirty();
  }

  protected onClose(): void {
    this.expenseFacade.clearSelectedCategory();
    this.detailsSidebarService.closeDrawer();
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const { name, parentId, color, displayOrder } = this.formGroup.getRawValue();
    const request = {
      name: (name ?? '').trim(),
      parentId: parentId ?? null,
      color: color ?? null,
      displayOrder: this.toOrder(displayOrder),
    };

    if (this.isCreateMode()) {
      this.expenseFacade.createCategory(request);
    } else {
      const current = this.selectedCategory();
      if (!current) {
        return;
      }

      this.expenseFacade.updateCategory(current.id, request);
    }

    this.onClose();
  }

  private enterCreateMode(): void {
    this.formGroup.reset({
      name: null,
      parentId: null,
      color: null,
      // Last in the list rather than first: a new category has no established
      // place in the P&L order, and 0 would push it above everything seeded.
      displayOrder: this.nextDisplayOrder(),
    });
  }

  private populateForm(category: ExpenseCategory | null): void {
    this.formGroup.reset({
      name: category?.name ?? null,
      parentId: category?.parentId ?? null,
      color: category?.color ?? null,
      displayOrder: category?.displayOrder ?? 0,
    });
  }

  /**
   * The order as an integer. A text input hands back a string (see
   * `ExpenseCategoryFormControls`), and an emptied field hands back `''` —
   * both of which the API's `int` would reject.
   */
  private toOrder(value: number | string): number {
    const parsed = Math.trunc(Number(value));

    return Number.isFinite(parsed) ? Math.max(0, parsed) : 0;
  }

  private nextDisplayOrder(): number {
    const orders = this.rows().map((row) => row.displayOrder);

    return orders.length ? Math.max(...orders) + 1 : 1;
  }

  private buildFormGroup(): FormGroup<ExpenseCategoryFormControls> {
    return new FormGroup<ExpenseCategoryFormControls>({
      name: new FormControl(null, [Validators.required, Validators.maxLength(100)]),
      parentId: new FormControl(null),
      // Seven characters is the API's cap: `#rrggbb` and nothing longer.
      color: new FormControl(null, [Validators.maxLength(7)]),
      displayOrder: new FormControl(0, { nonNullable: true, validators: [Validators.min(0)] }),
    });
  }
}
