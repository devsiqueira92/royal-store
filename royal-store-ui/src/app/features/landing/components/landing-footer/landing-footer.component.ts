import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { LandingFooterColumn } from '../../models/landing.model';

/** Site footer. Anchor links are reported upward so the page owns scrolling. */
@Component({
  selector: 'app-landing-footer',
  imports: [RouterLink, TranslatePipe],
  templateUrl: './landing-footer.component.html',
  styleUrl: './landing-footer.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingFooterComponent {
  readonly columns = input.required<readonly LandingFooterColumn[]>();
  readonly currentYear = input.required<number>();

  readonly navigateToSection = output<string>();

  protected onAnchorClick(event: Event, sectionId: string): void {
    event.preventDefault();
    this.navigateToSection.emit(sectionId);
  }
}
