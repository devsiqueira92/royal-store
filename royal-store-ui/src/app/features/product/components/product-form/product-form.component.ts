import {
  ChangeDetectionStrategy,
  Component,
  input,
  OnDestroy,
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
import { ProductFormControls } from '../../interfaces/product.interface';
import { ProductRequest } from '../../interfaces/product-request.interface';

export interface ProductCategoryOption {
  id: string;
  name: string;
}

const PRODUCT_SIZE_OPTIONS: readonly string[] = ['XS', 'S', 'M', 'L', 'XL'];

@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductFormComponent implements OnDestroy {
  categories = input<ProductCategoryOption[]>([]);

  protected readonly sizeOptions = PRODUCT_SIZE_OPTIONS;
  protected readonly selectedSizes = signal<string[]>([]);
  protected readonly imagePreviewUrl = signal<string | null>(null);
  protected readonly isDragOver = signal(false);

  protected readonly formGroup = new FormGroup<ProductFormControls>({
    id: new FormControl(null),
    categoryId: new FormControl(null, [Validators.required]),
    name: new FormControl(null, [
      Validators.required,
      Validators.maxLength(120),
    ]),
    sku: new FormControl(null, [Validators.maxLength(50)]),
    barcode: new FormControl(null, [Validators.maxLength(50)]),
    price: new FormControl(0, [Validators.required, Validators.min(0)]),
    costPrice: new FormControl(0, [Validators.min(0)]),
    stockQuantity: new FormControl(0, [
      Validators.required,
      Validators.min(0),
    ]),
    sizes: new FormControl<string[]>([]),
    isActive: new FormControl(true),
  });

  close = output<void>();
  submit = output<ProductRequest>();

  ngOnDestroy(): void {
    this.revokeImagePreview();
  }

  protected toggleSize(size: string): void {
    const next = this.selectedSizes().includes(size)
      ? this.selectedSizes().filter((selected) => selected !== size)
      : [...this.selectedSizes(), size];

    this.selectedSizes.set(next);
    this.formGroup.controls.sizes.setValue(next);
  }

  protected toggleActive(): void {
    const control = this.formGroup.controls.isActive;
    control.setValue(!control.value);
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(true);
  }

  protected onDragLeave(): void {
    this.isDragOver.set(false);
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(false);
    this.setImageFile(event.dataTransfer?.files?.[0]);
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.setImageFile(input.files?.[0]);
  }

  protected onClose(): void {
    this.close.emit();
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();

    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const {
      id,
      categoryId,
      name,
      sku,
      barcode,
      price,
      costPrice,
      stockQuantity,
      isActive,
    } = this.formGroup.getRawValue();

    this.submit.emit({
      id,
      categoryId: categoryId ?? undefined,
      name: (name ?? '').trim(),
      sku: sku?.trim() || undefined,
      barcode: barcode?.trim() || undefined,
      price: price ?? 0,
      costPrice: costPrice ?? undefined,
      stockQuantity: stockQuantity ?? 0,
      sizes: this.selectedSizes(),
      isActive: isActive ?? true,
    });
  }

  private setImageFile(file: File | undefined): void {
    if (!file) {
      return;
    }

    this.revokeImagePreview();
    this.imagePreviewUrl.set(URL.createObjectURL(file));
  }

  private revokeImagePreview(): void {
    const url = this.imagePreviewUrl();
    if (url) {
      URL.revokeObjectURL(url);
    }
  }
}
