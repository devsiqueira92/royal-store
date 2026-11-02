import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { LabelPrintPreviewComponent } from '../../components/label-print-preview/label-print-preview.component';
import { ProductFacade } from '../../facade/product.facade';
import { Product } from '../../interfaces/product.interface';
import { toLabelProducts } from '../../interfaces/label-print.interface';

@Component({
  imports: [TranslatePipe, RouterLink, LabelPrintPreviewComponent],
  templateUrl: './label-print.component.html',
  styleUrl: './label-print.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelPrintComponent {
  private readonly productFacade = inject(ProductFacade);

  private readonly selectedProducts = toSignal(
    this.productFacade.selectedLabelProducts$,
    { initialValue: [] as Product[] },
  );

  protected readonly labelProducts = computed(() =>
    toLabelProducts(this.selectedProducts()),
  );
}
