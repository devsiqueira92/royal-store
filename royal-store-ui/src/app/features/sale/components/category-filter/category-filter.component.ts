import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SelectButtonModule } from 'primeng/selectbutton';
import { Category } from '@app/features/category/interfaces/category.interface';

interface CategoryOption {
  label: string;
  value: string | null;
}

@Component({
  selector: 'app-category-filter',
  imports: [FormsModule, SelectButtonModule],
  templateUrl: './category-filter.component.html',
  styleUrl: './category-filter.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryFilterComponent {
  categories = input<Category[]>([]);
  selectedId = input<string | null>(null);
  allLabel = input<string>('All');

  selectedIdChange = output<string | null>();

  protected readonly options = computed<CategoryOption[]>(() => [
    { label: this.allLabel(), value: null },
    ...this.categories().map(category => ({
      label: category.name,
      value: category.id,
    })),
  ]);

  protected onChange(value: string | null): void {
    this.selectedIdChange.emit(value ?? null);
  }
}
