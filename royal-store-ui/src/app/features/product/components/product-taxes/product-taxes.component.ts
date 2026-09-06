import { Component, computed, inject, input } from '@angular/core';
import { FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IbptTaxAutocompleteComponent } from '@app/shared/components/form-fields/ibpt-tax-autocomplete/ibpt-tax-autocomplete.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { Options } from '@app/shared/interceptors/options.interface';
import { TranslatePipe } from '@ngx-translate/core';
import { CheckboxModule } from 'primeng/checkbox';
import { PanelModule } from 'primeng/panel';
import { ProductFormControls } from '../../interfaces/product.interface';
import { TaxTemplateSelectFieldComponent } from '@app/shared/components/form-fields/tax-template-select-field/tax-template-select-field.component';
import { TaxProfileTemplate } from '@app/core/tax-profile-template/interfaces/tax-profile-template.interface';
import { OriginSelectFieldComponent } from '@app/shared/components/form-fields/origin-select-field/origin-select-field.component';

@Component({
  selector: 'app-product-taxes',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    TranslatePipe,
    PanelModule,
    CheckboxModule,
    InputFieldComponent,
    IbptTaxAutocompleteComponent,
    FormLayoutComponent,
    TaxTemplateSelectFieldComponent,
    OriginSelectFieldComponent,
  ],
  templateUrl: './product-taxes.component.html',
  styleUrl: './product-taxes.component.scss',
})
export class ProductTaxesComponent {
  parentForm = input.required<FormGroup<ProductFormControls>>();
  itemType = input<number>(0);
  templates = input<TaxProfileTemplate[]>([]);
  helperText = input<string | null>(null);

  protected readonly isService = computed(() => this.itemType() === 1);

  protected readonly templateOptions = computed<Options[]>(() =>
    this.templates().map(template => ({
      code: template.id,
      description: template.name,
    })),
  );
}
