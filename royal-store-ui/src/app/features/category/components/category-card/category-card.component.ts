import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
} from '@angular/core';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { TranslatePipe } from '@ngx-translate/core';
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

  protected readonly countLabelKey = computed(() =>
    this.category().productCount === 1
      ? 'category.card.product'
      : 'category.card.products',
  );

  protected loadDetails(id: string) {
    this.categoryFacade.loadCategory(id);
    this.detailsSidebarService.openDrawer(EditCategoryComponent);
  }
}
