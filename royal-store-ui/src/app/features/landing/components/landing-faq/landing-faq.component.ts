import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { AccordionModule } from 'primeng/accordion';
import { LandingFaq } from '../../models/landing.model';
import { RevealOnScrollDirective } from '../../directives/reveal-on-scroll.directive';

/**
 * Objection-handling FAQ built on `p-accordion`, which supplies the
 * button/region aria pairing and keyboard operation. The first panel starts
 * open so the section never reads as an empty list of labels.
 */
@Component({
  selector: 'app-landing-faq',
  imports: [TranslatePipe, AccordionModule, RevealOnScrollDirective],
  templateUrl: './landing-faq.component.html',
  styleUrl: './landing-faq.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingFaqComponent {
  readonly sectionId = input.required<string>();
  readonly faqs = input.required<readonly LandingFaq[]>();
}
