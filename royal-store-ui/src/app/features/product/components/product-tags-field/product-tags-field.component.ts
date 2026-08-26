import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ProductCategoryOption } from '../../interfaces/product.interface';
import { getSwatchColor } from '@app/shared/utils/swatch-color.util';

@Component({
  selector: 'app-product-tags-field',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './product-tags-field.component.html',
  styleUrl: './product-tags-field.component.scss',
})
export class ProductTagsFieldComponent {
  categories = input<ProductCategoryOption[]>([]);
  label = input('product.form.tags');

  protected readonly selectedTags = signal<ProductCategoryOption[]>([]);
  protected readonly tagQuery = signal('');

  protected readonly getSwatchColor = getSwatchColor;

  tagsChange = output<ProductCategoryOption[]>();

  protected readonly tagSuggestions = computed(() => {
    const query = this.tagQuery().trim().toLowerCase();
    if (!query) {
      return [];
    }

    const selectedIds = new Set(this.selectedTags().map(tag => tag.id));

    return this.categories()
      .filter(
        category =>
          !selectedIds.has(category.id) &&
          category.name.toLowerCase().includes(query),
      )
      .slice(0, 5);
  });

  protected onTagInputKeydown(event: KeyboardEvent): void {
    if (event.key !== 'Enter') {
      return;
    }

    event.preventDefault();
    const [firstSuggestion] = this.tagSuggestions();
    if (firstSuggestion) {
      this.addTag(firstSuggestion);
    }
  }

  protected addTag(category: ProductCategoryOption): void {
    if (this.selectedTags().some(tag => tag.id === category.id)) {
      return;
    }

    this.selectedTags.update(tags => [...tags, category]);
    this.tagQuery.set('');
    this.tagsChange.emit(this.selectedTags());
  }

  protected removeTag(categoryId: string): void {
    this.selectedTags.update(tags =>
      tags.filter(tag => tag.id !== categoryId),
    );
    this.tagsChange.emit(this.selectedTags());
  }
}
