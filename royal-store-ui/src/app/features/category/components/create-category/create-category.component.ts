import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CategoryFormComponent } from '../category-form/category-form.component';
import { CategoryFacade } from '../../facade/category.facade';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { CategoryFormControls } from '../../interfaces/category.interface';
import { DataMode } from '@app/shared/enum/data-mode.enum';
import { CategoryRequest } from '../../interfaces/category-request.interface';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CategoryFormComponent, TranslatePipe],
  templateUrl: './create-category.component.html',
  styleUrl: './create-category.component.scss',
})
export class CreateCategoryComponent {
  private readonly categoryFacade = inject(CategoryFacade);
  protected readonly mode = DataMode;

  protected readonly formGroup = new FormGroup<CategoryFormControls>({
    id: new FormControl(null),
    name: new FormControl(null, [
      Validators.required,
      Validators.maxLength(100),
    ]),
    description: new FormControl(null, [Validators.maxLength(500)]),
    color: new FormControl(null, [Validators.maxLength(10)]),
  });

  protected onCreate(request: CategoryRequest): void {
    this.categoryFacade.createCategory(request);
    this.formGroup.reset();
  }
}
