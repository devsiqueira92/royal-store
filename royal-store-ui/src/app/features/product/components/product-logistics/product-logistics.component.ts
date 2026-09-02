import { Component, input } from '@angular/core';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { FormGroup } from '@angular/forms';
import { ProductFormControls } from '../../interfaces/product.interface';

@Component({
  selector: 'app-product-logistics',
  imports: [FormLayoutComponent, InputFieldComponent],
  templateUrl: './product-logistics.component.html',
  styleUrl: './product-logistics.component.scss',
})
export class ProductLogisticsComponent {
  formGroup = input.required<FormGroup<ProductFormControls>>();
}
