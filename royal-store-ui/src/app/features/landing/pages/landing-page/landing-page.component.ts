import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  DOCUMENT,
  afterNextRender,
  effect,
  inject,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { BillingCycle } from '../../models/landing.model';
import {
  LANDING_FAQS,
  LANDING_FEATURES,
  LANDING_FOOTER_COLUMNS,
  LANDING_NAV_LINKS,
  LANDING_PLANS,
  LANDING_SECTION_IDS,
  LANDING_SHOWCASE_TABS,
  LANDING_STATS,
} from '../../data/landing.content';
import { LandingNavbarComponent } from '../../components/landing-navbar/landing-navbar.component';
import { LandingHeroComponent } from '../../components/landing-hero/landing-hero.component';
import { LandingStatsComponent } from '../../components/landing-stats/landing-stats.component';
import { LandingFeaturesComponent } from '../../components/landing-features/landing-features.component';
import { LandingShowcaseComponent } from '../../components/landing-showcase/landing-showcase.component';
import { LandingPricingComponent } from '../../components/landing-pricing/landing-pricing.component';
import { LandingFaqComponent } from '../../components/landing-faq/landing-faq.component';
import { LandingFinalCtaComponent } from '../../components/landing-final-cta/landing-final-cta.component';
import { LandingFooterComponent } from '../../components/landing-footer/landing-footer.component';

/**
 * Public marketing page (route `/`).
 *
 * Smart component: owns the page-level state (billing cycle, which section is
 * in view), the smooth-scroll navigation and the document title. Every section
 * below is presentational and receives data through signal inputs.
 */
@Component({
  selector: 'app-landing-page',
  imports: [
    TranslatePipe,
    LandingNavbarComponent,
    LandingHeroComponent,
    LandingStatsComponent,
    LandingFeaturesComponent,
    LandingShowcaseComponent,
    LandingPricingComponent,
    LandingFaqComponent,
    LandingFinalCtaComponent,
    LandingFooterComponent,
  ],
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingPageComponent {
  protected readonly sectionIds = LANDING_SECTION_IDS;
  protected readonly navLinks = LANDING_NAV_LINKS;
  protected readonly stats = LANDING_STATS;
  protected readonly features = LANDING_FEATURES;
  protected readonly showcaseTabs = LANDING_SHOWCASE_TABS;
  protected readonly plans = LANDING_PLANS;
  protected readonly faqs = LANDING_FAQS;
  protected readonly footerColumns = LANDING_FOOTER_COLUMNS;
  protected readonly currentYear = new Date().getFullYear();

  /** Annual is pre-selected: it is the cheaper, recommended commitment. */
  protected readonly billingCycle = signal<BillingCycle>('annual');
  protected readonly activeSection = signal<string | null>(null);

  private readonly document = inject(DOCUMENT);
  private readonly title = inject(Title);
  private readonly translate = inject(TranslateService);

  /** Follows the active language, so the tab title changes with it too. */
  private readonly pageTitle = toSignal(this.translate.stream('landing.meta.title'));

  constructor() {
    const destroyRef = inject(DestroyRef);

    effect(() => {
      const pageTitle = this.pageTitle();

      if (typeof pageTitle === 'string' && pageTitle.length > 0) {
        this.title.setTitle(pageTitle);
      }
    });

    afterNextRender(() => {
      const observer = this.observeSections();
      destroyRef.onDestroy(() => observer?.disconnect());
    });
  }

  protected setBillingCycle(cycle: BillingCycle): void {
    this.billingCycle.set(cycle);
  }

  /**
   * Scrolls to a section and moves focus there, so keyboard and screen-reader
   * users land where sighted users do. Honours `prefers-reduced-motion` by
   * jumping instead of animating.
   */
  protected scrollToSection(sectionId: string): void {
    const target = this.document.getElementById(sectionId);

    if (!target) {
      return;
    }

    const prefersReducedMotion = this.document.defaultView?.matchMedia?.(
      '(prefers-reduced-motion: reduce)',
    ).matches;

    target.scrollIntoView({
      behavior: prefersReducedMotion ? 'auto' : 'smooth',
      block: 'start',
    });
    target.focus({ preventScroll: true });
  }

  /** Tracks the section in view to keep the navbar's active link in sync. */
  private observeSections(): IntersectionObserver | null {
    if (typeof IntersectionObserver === 'undefined') {
      return null;
    }

    const observer = new IntersectionObserver(
      entries => {
        const visible = entries
          .filter(entry => entry.isIntersecting)
          .sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];

        if (visible) {
          this.activeSection.set(visible.target.id);
        }
      },
      { rootMargin: '-20% 0px -60% 0px', threshold: [0.1, 0.35] },
    );

    for (const id of Object.values(LANDING_SECTION_IDS)) {
      const element = this.document.getElementById(id);
      if (element) {
        observer.observe(element);
      }
    }

    return observer;
  }
}
