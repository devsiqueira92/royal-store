import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TableModule } from 'primeng/table';
import { TranslatePipe } from '@ngx-translate/core';
import { SaleItemDetails } from '../../interfaces/sale-item-details.interface';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-sale-items',
  imports: [TableModule, CurrencyPipe, TranslatePipe],
  templateUrl: './sale-items.component.html',
  styleUrl: './sale-items.component.scss',
})
export class SaleItemsComponent {
  readonly items = input.required<SaleItemDetails[]>();
}
