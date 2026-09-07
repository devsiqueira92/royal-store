import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

export interface CheckoutStep {
  index: number;
  label: string;
  title: string;
}

@Component({
  selector: 'app-checkout-steps',
  imports: [TranslatePipe],
  templateUrl: './checkout-steps.component.html',
  styleUrl: './checkout-steps.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CheckoutStepsComponent {
  steps = input.required<CheckoutStep[]>();
  current = input.required<number>();
}
