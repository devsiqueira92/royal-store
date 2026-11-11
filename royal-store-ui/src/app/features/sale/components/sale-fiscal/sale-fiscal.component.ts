import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TableModule } from 'primeng/table';
import { TranslatePipe } from '@ngx-translate/core';
import { SaleItemDetails } from '../../interfaces/sale-item-details.interface';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-sale-fiscal',
  imports: [TableModule, TranslatePipe],
  templateUrl: './sale-fiscal.component.html',
  styleUrl: './sale-fiscal.component.scss',
})
export class SaleFiscalComponent {
  readonly items = input.required<SaleItemDetails[]>();
}
