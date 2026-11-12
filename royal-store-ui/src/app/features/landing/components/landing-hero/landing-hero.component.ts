import { ChangeDetectionStrategy, Component, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { RippleModule } from 'primeng/ripple';
import { DashboardMockupComponent } from '../dashboard-mockup/dashboard-mockup.component';
import { RevealOnScrollDirective } from '../../directives/reveal-on-scroll.directive';

/** Above-the-fold hero: pain-first headline, single primary CTA, product peek. */
@Component({
  selector: 'app-landing-hero',
  imports: [
    RouterLink,
    TranslatePipe,
    RippleModule,
    DashboardMockupComponent,
    RevealOnScrollDirective,
  ],
  templateUrl: './landing-hero.component.html',
  styleUrl: './landing-hero.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingHeroComponent {
  /** Asks the page to scroll to the interactive demo section. */
  readonly showDemo = output<void>();
}
