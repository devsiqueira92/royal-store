import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TagModule } from 'primeng/tag';
import { TranslatePipe } from '@ngx-translate/core';
import { SaleDetails } from '../../interfaces/sale-details.interface';

export type SaleParties = Pick<
  SaleDetails,
  | 'fulfillmentMode'
  | 'sellerName'
  | 'customerName'
  | 'customerLoyaltyTierName'
  | 'soldAt'
>;

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-sale-parties',
  imports: [DatePipe, TagModule, TranslatePipe],
  templateUrl: './sale-parties.component.html',
  styleUrl: './sale-parties.component.scss',
})
export class SalePartiesComponent {
  readonly parties = input.required<SaleParties>();

  protected readonly dateFormat = 'HH:mm · MMM d, y';

  protected fulfillmentModeSeverity(fulfillmentMode: string): 'info' | 'warn' {
    return fulfillmentMode.toLowerCase() === 'pickup' ? 'warn' : 'info';
  }
}
