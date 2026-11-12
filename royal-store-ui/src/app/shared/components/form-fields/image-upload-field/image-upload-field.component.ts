import {
  ChangeDetectionStrategy,
  Component,
  input,
  OnDestroy,
  output,
  signal,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

export interface UploadedImage {
  url: string;
  file?: File;
  isCover: boolean;
}

@Component({
  selector: 'app-image-upload-field',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [TranslatePipe],
  templateUrl: './image-upload-field.component.html',
  styleUrl: './image-upload-field.component.scss',
})
export class ImageUploadFieldComponent implements OnDestroy {
  label = input('product.form.images');
  hintKey = input('product.form.imagesHint');
  addLabelKey = input('product.form.addImage');
  coverLabelKey = input('product.form.cover');

  protected readonly images = signal<UploadedImage[]>([]);

  filesChange = output<File[]>();

  ngOnDestroy(): void {
    for (const image of this.images()) {
      if (image.file) {
        URL.revokeObjectURL(image.url);
      }
    }
  }

  protected onImagesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.addImageFiles(input.files);
    input.value = '';
  }

  protected onImagesDropped(event: DragEvent): void {
    event.preventDefault();
    this.addImageFiles(event.dataTransfer?.files ?? null);
  }

  protected setCoverImage(index: number): void {
    this.images.update(images => images.map((image, i) => ({ ...image, isCover: i === index })));
  }

  protected removeImage(index: number, event: Event): void {
    event.stopPropagation();

    const removed = this.images()[index];
    this.images.update(images => {
      const next = images.filter((_, i) => i !== index);
      if (removed?.isCover && next.length > 0) {
        next[0] = { ...next[0], isCover: true };
      }
      return next;
    });

    if (removed?.file) {
      URL.revokeObjectURL(removed.url);
    }

    this.emitFiles();
  }

  private addImageFiles(fileList: FileList | null): void {
    if (!fileList || fileList.length === 0) {
      return;
    }

    const newImages: UploadedImage[] = Array.from(fileList).map(file => ({
      url: URL.createObjectURL(file),
      file,
      isCover: false,
    }));

    this.images.update(images => {
      const merged = [...images, ...newImages];
      if (merged.length > 0 && !merged.some(image => image.isCover)) {
        merged[0] = { ...merged[0], isCover: true };
      }
      return merged;
    });

    this.emitFiles();
  }

  private emitFiles(): void {
    this.filesChange.emit(
      this.images()
        .map(image => image.file)
        .filter((file): file is File => !!file),
    );
  }
}
