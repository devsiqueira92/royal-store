import { ChangeDetectionStrategy, Component, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { RippleModule } from 'primeng/ripple';
import { RevealOnScrollDirective } from '../../directives/reveal-on-scroll.directive';

/** Closing conversion block: one strong CTA plus a low-commitment alternative. */
@Component({
  selector: 'app-landing-final-cta',
  imports: [RouterLink, TranslatePipe, RippleModule, RevealOnScrollDirective],
  templateUrl: './landing-final-cta.component.html',
  styleUrl: './landing-final-cta.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingFinalCtaComponent {
  readonly viewPlans = output<void>();
}
