import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
  signal,
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { CATEGORY_COLOR_PALETTE } from '../../constants/category-colors.constant';
import {
  Category,
  CategoryFormControls,
} from '../../interfaces/category.interface';
import { CategoryRequest } from '../../interfaces/category-request.interface';

@Component({
  selector: 'app-category-form',
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './category-form.component.html',
  styleUrl: './category-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryFormComponent {
  category = input<Category>();
  mode = input.required<string>();
  parentForm = input.required<FormGroup<CategoryFormControls>>();

  protected readonly colorPalette = CATEGORY_COLOR_PALETTE;
  protected readonly selectedColor = signal(CATEGORY_COLOR_PALETTE[0]);

  submit = output<CategoryRequest>();

  protected selectColor(color: string): void {
    this.selectedColor.set(color);
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();

    if (this.parentForm().invalid) {
      this.parentForm().markAllAsTouched();
      return;
    }

    const { id, name, description } = this.parentForm().getRawValue();
    const trimmedDescription = description?.trim();

    this.submit.emit({
      id,
      name: (name ?? '').trim(),
      description: trimmedDescription ? trimmedDescription : undefined,
      color: this.selectedColor(),
    });
  }
}
