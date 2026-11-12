import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { LandingFeature } from '../../models/landing.model';
import { RevealOnScrollDirective } from '../../directives/reveal-on-scroll.directive';

/** Asymmetric bento grid of capabilities. Purely presentational. */
@Component({
  selector: 'app-landing-features',
  imports: [TranslatePipe, RevealOnScrollDirective],
  templateUrl: './landing-features.component.html',
  styleUrl: './landing-features.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingFeaturesComponent {
  readonly sectionId = input.required<string>();
  readonly features = input.required<readonly LandingFeature[]>();
}
