import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  output,
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { RadioButtonModule } from 'primeng/radiobutton';
import { ItemType } from '../../interfaces/product.interface';
import { ProductFacade } from '../../facade/product.facade';
import { take, tap } from 'rxjs';
import { TaxProfileTemplateFacade } from '@app/core/tax-profile-template/facade/tax-profile-template.facade';

interface ItemTypeOption {
  value: ItemType;
  titleKey: string;
  subtitleKey: string;
}

const OPTIONS: ItemTypeOption[] = [
  {
    value: 0,
    titleKey: 'product.form.itemType.physical',
    subtitleKey: 'product.form.itemType.physicalHint',
  },
  {
    value: 1,
    titleKey: 'product.form.itemType.service',
    subtitleKey: 'product.form.itemType.serviceHint',
  },
];

@Component({
  selector: 'app-item-type-selector',
  imports: [ReactiveFormsModule, TranslatePipe, RadioButtonModule],
  templateUrl: './item-type-selector.component.html',
  styleUrl: './item-type-selector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ItemTypeSelectorComponent {
  private readonly taxProfileTemplateFacade = inject(TaxProfileTemplateFacade);
  private readonly productFacade = inject(ProductFacade);
  private readonly selectedProduct$ = this.productFacade.selectedProduct$;

  protected readonly formGroup = new FormGroup({
    itemType: new FormControl(0, { nonNullable: true }),
  });

  protected get value(): number {
    return this.formGroup.controls.itemType.value;
  }
  disabled = input<boolean>(false);
  valueChange = output<ItemType>();

  protected readonly options = OPTIONS;

  constructor() {
    this.selectedProduct$
      .pipe(
        take(1),
        tap(product => {
          const itemType = product?.type || 0;
          product !== null
            ? this.getTaxTemplate(itemType as ItemType)
            : this.getTaxTemplate(0);

          this.formGroup.patchValue({ itemType: itemType || 0 });
          this.valueChange.emit(itemType as ItemType);
        }),
      )
      .subscribe();
  }

  protected select(value: ItemType): void {
    if (!this.disabled()) {
      this.getTaxTemplate(value);
      this.formGroup.patchValue({ itemType: value });
      this.valueChange.emit(value);
    }
  }

  private getTaxTemplate(value: ItemType) {
    this.taxProfileTemplateFacade.loadTemplates(value);
  }
}
