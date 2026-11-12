import { ChangeDetectionStrategy, Component, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { TabsModule } from 'primeng/tabs';
import { LandingShowcaseTab } from '../../models/landing.model';
import { DashboardMockupComponent } from '../dashboard-mockup/dashboard-mockup.component';
import { RevealOnScrollDirective } from '../../directives/reveal-on-scroll.directive';

/**
 * "Inside the system" section: PrimeNG tabs switch between simulated screens.
 *
 * `p-tabs` brings the roving-tabindex keyboard model and the aria wiring, so the
 * only state here is which tab is selected. The panels are kept eager (`lazy`
 * off) because each one is a light static mockup — swapping tabs then costs no
 * layout jump and no spinner.
 */
@Component({
  selector: 'app-landing-showcase',
  imports: [
    RouterLink,
    TranslatePipe,
    TabsModule,
    DashboardMockupComponent,
    RevealOnScrollDirective,
  ],
  templateUrl: './landing-showcase.component.html',
  styleUrl: './landing-showcase.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingShowcaseComponent {
  readonly sectionId = input.required<string>();
  readonly tabs = input.required<readonly LandingShowcaseTab[]>();

  protected readonly activeTab = signal<string>('pos');

  protected onTabChange(value: string | number | undefined): void {
    if (typeof value === 'string') {
      this.activeTab.set(value);
    }
  }
}
