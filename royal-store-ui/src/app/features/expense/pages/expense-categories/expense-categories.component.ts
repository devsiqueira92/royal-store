import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { ExpenseFacade } from '../../facade/expense.facade';
import { ExpenseCategoryRow } from '../../interfaces/expense.interface';
import { ExpenseCategoryListComponent } from '../../components/expense-category-list/expense-category-list.component';
import { ExpenseCategoryFormComponent } from '../../components/expense-category-form/expense-category-form.component';

/**
 * Expense categories: the buckets the management P&L rolls up.
 *
 * The smart component of the screen. It owns the inactive filter, opens the
 * drawer and confirms the delete; the table below it is presentational.
 *
 * Inactive categories are shown by default. This is the only place they can be
 * brought back, and a management screen that hides part of what it manages is
 * how a category ends up created twice.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, TranslatePipe, ButtonModule, SwitchComponent, ExpenseCategoryListComponent],
  templateUrl: './expense-categories.component.html',
  styleUrl: './expense-categories.component.scss',
})
export class ExpenseCategoriesComponent implements OnInit {
  private readonly expenseFacade = inject(ExpenseFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translateService = inject(TranslateService);

  protected readonly showInactive = signal(true);

  protected readonly rows = toSignal(this.expenseFacade.categoryRows$, {
    initialValue: [] as ExpenseCategoryRow[],
  });

  protected readonly isLoading = toSignal(this.expenseFacade.isLoadingCategories$, {
    initialValue: false,
  });

  protected readonly isSaving = toSignal(this.expenseFacade.isSaving$, { initialValue: false });

  protected readonly activeCount = computed(() => this.rows().filter((row) => row.isActive).length);

  protected readonly inactiveCount = computed(() => this.rows().length - this.activeCount());

  ngOnInit(): void {
    this.expenseFacade.loadCategories(this.showInactive());
  }

  protected onShowInactiveChange(show: boolean): void {
    this.showInactive.set(show);
    this.expenseFacade.loadCategories(show);
  }

  protected onCreate(): void {
    this.expenseFacade.clearSelectedCategory();
    this.detailsSidebarService.openDrawer(ExpenseCategoryFormComponent, {
      title: 'expense.categories.form.create',
      subtitle: 'expense.categories.form.subtitle',
      mode: 'create',
    });
  }

  protected onEdit(row: ExpenseCategoryRow): void {
    this.expenseFacade.selectCategory(row);
    this.detailsSidebarService.openDrawer(ExpenseCategoryFormComponent, {
      title: 'expense.categories.form.edit',
      subtitle: 'expense.categories.form.subtitle',
      mode: 'edit',
    });
  }

  protected onActiveChange({ id, active }: { id: string; active: boolean }): void {
    this.expenseFacade.setCategoryActive(id, active);
  }

  /**
   * Deleting is permanent and the API only allows it while nothing references
   * the category, so it asks first — anchored to the button that was pressed,
   * as everywhere else in the app. A category already in use comes back as a
   * 409 whose message the global error toast shows.
   */
  protected onDelete({ row, event }: { row: ExpenseCategoryRow; event: Event }): void {
    this.confirmationService.confirm({
      target: event.currentTarget as EventTarget,
      message: this.translateService.instant('expense.categories.list.deleteConfirm', {
        name: row.path,
      }),
      icon: 'pi pi-info-circle',
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', outlined: true },
      acceptLabel: this.translateService.instant('expense.categories.list.deleteAccept'),
      rejectLabel: this.translateService.instant('expense.categories.list.deleteReject'),
      accept: () => this.expenseFacade.deleteCategory(row.id),
    });
  }
}
