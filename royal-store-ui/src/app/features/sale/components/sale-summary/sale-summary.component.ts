import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
} from '@angular/core';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectButtonModule } from 'primeng/selectbutton';
import { DiscountMode } from '../../interfaces/sale-totals.interface';

interface ModeOption {
  label: string;
  value: DiscountMode;
}

const MODE_OPTIONS: ModeOption[] = [
  { label: '$', value: 'amount' },
  { label: '%', value: 'percent' },
];

@Component({
  selector: 'app-sale-summary',
  imports: [
    CurrencyPipe,
    DecimalPipe,
    FormsModule,
    TranslatePipe,
    InputNumberModule,
    SelectButtonModule,
  ],
  templateUrl: './sale-summary.component.html',
  styleUrl: './sale-summary.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SaleSummaryComponent {
  subtotal = input<number>(0);
  discountValue = input<number>(0);
  discountMode = input<DiscountMode>('amount');
  discountAmount = input<number>(0);
  taxPercent = input<number>(0);
  tax = input<number>(0);
  total = input<number>(0);

  discountValueChange = output<number>();
  discountModeChange = output<DiscountMode>();

  protected readonly modeOptions = MODE_OPTIONS;

  protected onValueChange(value: number | null): void {
    this.discountValueChange.emit(value && value > 0 ? value : 0);
  }
}
