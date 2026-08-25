import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { filter, take, tap } from 'rxjs';
import { CATEGORY_COLOR_PALETTE } from '../../constants/category-colors.constant';
import { CategoryFacade } from '../../facade/category.facade';
import { CategoryFormComponent } from '../category-form/category-form.component';
import { AsyncPipe } from '@angular/common';
import { CategoryFormControls } from '../../interfaces/category.interface';
import { DataMode } from '@app/shared/enum/data-mode.enum';
import { CategoryRequest } from '../../interfaces/category-request.interface';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CategoryFormComponent, AsyncPipe],
  templateUrl: './edit-category.component.html',
  styleUrl: './edit-category.component.scss',
})
export class EditCategoryComponent {
  private readonly categoryFacade = inject(CategoryFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly selectedCateogry$ =
    this.categoryFacade.selectedCategory$.pipe(
      filter(Boolean),
      takeUntilDestroyed(),
      tap(category => this.formGroup.patchValue({ ...category })),
    );

  protected readonly loading$ = this.categoryFacade.isLoading$;
  protected readonly selectedColor = signal(CATEGORY_COLOR_PALETTE[0]);

  protected readonly mode = DataMode;

  formGroup = new FormGroup<CategoryFormControls>({
    id: new FormControl(null),
    name: new FormControl(null, [
      Validators.required,
      Validators.maxLength(100),
    ]),
    description: new FormControl(null, [Validators.maxLength(500)]),
    color: new FormControl(null),
  });

  protected onUpdate(request: CategoryRequest): void {
    this.categoryFacade.updateCategory(request.id!, request);
    this.formGroup.reset();
    this.detailsSidebarService.closeDrawer();
  }
}
