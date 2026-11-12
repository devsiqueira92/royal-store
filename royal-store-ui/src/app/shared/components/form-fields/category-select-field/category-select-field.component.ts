import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { ReactiveFormsModule, UntypedFormGroup, Validators } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { FormErrorComponent } from '../form-error/form-error.component';
import { Options } from '@app/shared/interceptors/options.interface';
import { CategoryFacade } from '@app/features/category/facade/category.facade';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { ProductCategoryOption } from '@app/features/product/interfaces/product.interface';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '../../button/button.component';
import { DividerModule } from 'primeng/divider';
import { DialogManagerService } from '@app/shared/services/dialog.service';
import { CreateCategoryComponent } from '@app/features/category/components/create-category/create-category.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-category-select-field',
  imports: [
    ReactiveFormsModule,
    SelectModule,
    FloatLabelModule,
    TranslatePipe,
    FormErrorComponent,
    CommonModule,
    ButtonComponent,
    DividerModule,
  ],
  templateUrl: './category-select-field.component.html',
  styleUrl: './category-select-field.component.scss',
})
export class CategorySelectFieldComponent {
  private readonly categoryFacade = inject(CategoryFacade);
  private readonly dialogService = inject(DialogManagerService);
  private readonly translateService = inject(TranslateService);
  protected readonly isLoading$ = this.categoryFacade.isLoading$;
  protected readonly categories = toSignal(
    this.categoryFacade.categories$.pipe(
      map(categories =>
        categories.map((category): ProductCategoryOption => ({
          id: category.id,
          name: category.name,
          color: category.color,
        })),
      ),
    ),
    { initialValue: [] as ProductCategoryOption[] },
  );

  parentForm = input.required<UntypedFormGroup>();
  controlName = input.required<string>();
  label = input<string>();

  options = input<Options[]>([]);
  loading = input<boolean>();
  showClear = input<boolean>();
  filter = input<boolean>();

  onSelect = output<SelectChangeEvent>();
  Validators = Validators;

  constructor() {
    this.categoryFacade.loadCategories();
  }

  openCategoryModal() {
    this.dialogService.open(CreateCategoryComponent, {
      header: this.translateService.instant('category.form.title'),
      closable: true,
    });
  }
}
