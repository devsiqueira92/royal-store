import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { LandingStat } from '../../models/landing.model';
import { RevealOnScrollDirective } from '../../directives/reveal-on-scroll.directive';

/** Social-proof band: impact numbers directly under the hero. */
@Component({
  selector: 'app-landing-stats',
  imports: [TranslatePipe, RevealOnScrollDirective],
  templateUrl: './landing-stats.component.html',
  styleUrl: './landing-stats.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingStatsComponent {
  readonly stats = input.required<readonly LandingStat[]>();
}
