import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { RippleModule } from 'primeng/ripple';
import { BillingCycle, LandingPlan } from '../../models/landing.model';
import { RevealOnScrollDirective } from '../../directives/reveal-on-scroll.directive';

/**
 * Pricing table with a monthly/annual switch.
 *
 * The switch is a two-button group with `aria-pressed`, not a styled div: it
 * keeps a native control, a visible label per option and real keyboard
 * operation, and it announces which cycle is active instead of relying on the
 * highlight alone.
 */
@Component({
  selector: 'app-landing-pricing',
  imports: [RouterLink, TranslatePipe, RippleModule, RevealOnScrollDirective],
  templateUrl: './landing-pricing.component.html',
  styleUrl: './landing-pricing.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingPricingComponent {
  readonly sectionId = input.required<string>();
  readonly plans = input.required<readonly LandingPlan[]>();
  readonly billingCycle = input.required<BillingCycle>();

  readonly billingCycleChange = output<BillingCycle>();

  protected priceOf(plan: LandingPlan): number | null {
    return this.billingCycle() === 'annual' ? plan.annualPrice : plan.monthlyPrice;
  }
}
