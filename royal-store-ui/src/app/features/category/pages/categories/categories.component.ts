import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { combineLatest, map } from 'rxjs';
import { TableModule } from 'primeng/table';
import { TranslatePipe } from '@ngx-translate/core';
import { CategoryFacade } from '../../facade/category.facade';
import { ProductFacade } from '@app-features/product/facade/product.facade';

interface CategoryRow {
  id: string;
  name: string;
  description?: string;
  numberOfItems: number;
}

@Component({
  imports: [TableModule, TranslatePipe],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoriesComponent implements OnInit {
  private categoryFacade = inject(CategoryFacade);
  private productFacade = inject(ProductFacade);

  protected readonly categories = toSignal(
    combineLatest([
      this.categoryFacade.categories$,
      this.productFacade.products$,
    ]).pipe(
      map(([categories, products]) =>
        categories.map(
          (category): CategoryRow => ({
            id: category.id,
            name: category.name,
            description: category.description,
            numberOfItems: products.filter(
              product => product.categoryId === category.id,
            ).length,
          }),
        ),
      ),
    ),
    { initialValue: [] },
  );

  protected readonly isLoading = toSignal(this.categoryFacade.isLoading$, {
    initialValue: false,
  });

  ngOnInit(): void {
    this.categoryFacade.loadCategories();
    this.productFacade.loadProducts();
  }
}
