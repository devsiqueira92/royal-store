import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { map } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { CategoryFacade } from '../../facade/category.facade';
import { CategoryFormComponent } from '../../components/category-form/category-form.component';
import {
  CategoryCardComponent,
  CategoryCardItem,
} from '../../components/category-card/category-card.component';
import { CATEGORY_COLOR_PALETTE } from '../../constants/category-colors.constant';
import { RouterOutlet } from '@angular/router';

@Component({
  imports: [AsyncPipe, TranslatePipe, CategoryCardComponent, RouterOutlet],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoriesComponent implements OnInit {
  private categoryFacade = inject(CategoryFacade);

  protected readonly categoryCards$ = this.categoryFacade.categories$.pipe(
    map((categories): CategoryCardItem[] =>
      categories.map((category, index) => ({
        id: category.id,
        name: category.name,
        productCount: category.productCount,
        color:
          category.color ??
          CATEGORY_COLOR_PALETTE[index % CATEGORY_COLOR_PALETTE.length],
      })),
    ),
  );

  protected readonly isLoading$ = this.categoryFacade.isLoading$;

  ngOnInit(): void {
    this.categoryFacade.loadCategories();
  }
}
