import { Component, computed, input } from '@angular/core';
import { FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IbptTaxAutocompleteComponent } from '@app/features/ibpt-tax/components/ibpt-tax-autocomplete/ibpt-tax-autocomplete.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { Options } from '@app/shared/interceptors/options.interface';
import { TranslatePipe } from '@ngx-translate/core';
import { CheckboxModule } from 'primeng/checkbox';
import { PanelModule } from 'primeng/panel';
import { TaxProfileTemplate } from '../../data/tax-profile-templates';
import { ProductFormControls } from '../../interfaces/product.interface';

const ORIGIN_OPTIONS: Options[] = [
  { code: '0', description: '0 — National' },
  { code: '1', description: '1 — Foreign · direct import' },
  { code: '2', description: '2 — Foreign · acquired domestically' },
  { code: '3', description: '3 — National · import content 40–70%' },
  { code: '4', description: '4 — National · Basic Productive Process' },
  { code: '5', description: '5 — National · import content ≤ 40%' },
  { code: '6', description: '6 — Foreign · direct import, no similar' },
  { code: '7', description: '7 — Foreign · domestic market, no similar' },
  { code: '8', description: '8 — National · import content > 70%' },
];

@Component({
  selector: 'app-product-taxes',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    TranslatePipe,
    PanelModule,
    CheckboxModule,
    InputFieldComponent,
    SelectFieldComponent,
    IbptTaxAutocompleteComponent,
    FormLayoutComponent,
  ],
  templateUrl: './product-taxes.component.html',
  styleUrl: './product-taxes.component.scss',
})
export class ProductTaxesComponent {
  parentForm = input.required<FormGroup<ProductFormControls>>();
  itemType = input<number>(0);
  templates = input<TaxProfileTemplate[]>([]);
  helperText = input<string | null>(null);

  protected readonly originOptions = ORIGIN_OPTIONS;

  protected readonly isService = computed(() => this.itemType() === 1);

  protected readonly templateOptions = computed<Options[]>(() =>
    this.templates().map(template => ({
      code: template.code,
      description: template.label,
    })),
  );
}
