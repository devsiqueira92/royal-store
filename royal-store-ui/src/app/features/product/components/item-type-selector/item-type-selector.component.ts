import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { RadioButtonModule } from 'primeng/radiobutton';
import { ItemType } from '../../interfaces/product.interface';

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
  imports: [FormsModule, TranslatePipe, RadioButtonModule],
  templateUrl: './item-type-selector.component.html',
  styleUrl: './item-type-selector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ItemTypeSelectorComponent {
  value = input<ItemType>(0);
  disabled = input<boolean>(false);

  valueChange = output<ItemType>();

  protected readonly options = OPTIONS;

  protected select(value: ItemType): void {
    if (!this.disabled() && value !== this.value()) {
      this.valueChange.emit(value);
    }
  }
}
