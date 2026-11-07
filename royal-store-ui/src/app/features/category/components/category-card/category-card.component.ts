import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
} from '@angular/core';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { ConfirmationService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { CategoryFacade } from '../../facade/category.facade';
import { EditCategoryComponent } from '../edit-category/edit-category.component';

export interface CategoryCardItem {
  id: string;
  name: string;
  productCount: number;
  color: string;
}

@Component({
  selector: 'app-category-card',
  imports: [TranslatePipe],
  templateUrl: './category-card.component.html',
  styleUrl: './category-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryCardComponent {
  category = input.required<CategoryCardItem>();
  private categoryFacade = inject(CategoryFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translateService = inject(TranslateService);

  protected readonly countLabelKey = computed(() =>
    this.category().productCount === 1
      ? 'category.card.product'
      : 'category.card.products',
  );

  protected loadDetails(id: string) {
    this.categoryFacade.loadCategory(id);
    this.detailsSidebarService.openDrawer(EditCategoryComponent, {
      title: 'category.form.edit',
    });
  }

  protected deleteCategory(event: Event) {
    const { id, name } = this.category();
    this.confirmationService.confirm({
      target: event.currentTarget as EventTarget,
      message: this.translateService.instant('category.deleteConfirm', { name }),
      icon: 'pi pi-info-circle',
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', outlined: true },
      acceptLabel: this.translateService.instant('category.form.delete'),
      rejectLabel: this.translateService.instant('category.form.cancel'),
      accept: () => this.categoryFacade.deleteCategory(id),
    });
  }
}
