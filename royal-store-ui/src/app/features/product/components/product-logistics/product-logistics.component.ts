import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
} from '@angular/core';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { FormGroup } from '@angular/forms';
import { ProductFormControls } from '../../interfaces/product.interface';
import { ProductFacade } from '../../facade/product.facade';
import { take, tap } from 'rxjs';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';

@Component({
  selector: 'app-product-logistics',
  imports: [
    FormLayoutComponent,
    InputFieldComponent,
    InputNumberFieldComponent,
  ],
  templateUrl: './product-logistics.component.html',
  styleUrl: './product-logistics.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductLogisticsComponent {
  private readonly productFacade = inject(ProductFacade);
  formGroup = input.required<FormGroup<ProductFormControls>>();

  ngOnInit() {
    this.productFacade.selectedProduct$
      .pipe(
        take(1),
        tap(product => this.formGroup().patchValue({ ...product?.measure })),
      )
      .subscribe();
  }
}
