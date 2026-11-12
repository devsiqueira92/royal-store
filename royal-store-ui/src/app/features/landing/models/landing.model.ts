/**
 * Presentation models for the public landing page.
 *
 * Every user-facing string lives in `assets/i18n/*.json`; the models below carry
 * only translation KEYS plus structural/visual metadata. That keeps the landing
 * copy translatable (pt/en) without pushing text into component code.
 */

/** Smooth-scroll anchor in the fixed navbar. `id` doubles as the section's DOM id. */
export interface LandingNavLink {
  readonly id: string;
  readonly labelKey: string;
}

/** Social-proof number shown in the band under the hero. */
export interface LandingStat {
  readonly id: string;
  readonly icon: string;
  readonly valueKey: string;
  readonly labelKey: string;
}

/** Span of a bento card on the 4-column desktop grid. */
export type LandingFeatureSpan = 'hero' | 'wide' | 'normal';

export interface LandingFeature {
  readonly id: string;
  readonly icon: string;
  readonly span: LandingFeatureSpan;
  readonly titleKey: string;
  readonly descriptionKey: string;
  /** Optional pill rendered above the title (e.g. "Mais usado"). */
  readonly tagKey?: string;
  /** Bullet list, only rendered for the hero card. */
  readonly bulletKeys?: readonly string[];
}

/** Which simulated product screen the dashboard mockup renders. */
export type MockupVariant = 'overview' | 'pos' | 'inventory' | 'finance';

/** One tab of the "inside the system" section. */
export interface LandingShowcaseTab {
  readonly id: string;
  readonly icon: string;
  readonly variant: MockupVariant;
  readonly labelKey: string;
  readonly titleKey: string;
  readonly descriptionKey: string;
  /** Alt text for the simulated screen shown in this tab. */
  readonly mockupAltKey: string;
  readonly bulletKeys: readonly string[];
}

export type BillingCycle = 'monthly' | 'annual';

export interface LandingPlan {
  readonly id: string;
  readonly nameKey: string;
  readonly descriptionKey: string;
  /** `null` renders the "custom pricing" treatment instead of a number. */
  readonly monthlyPrice: number | null;
  /** Per-month price when billed annually. */
  readonly annualPrice: number | null;
  readonly featureKeys: readonly string[];
  readonly ctaKey: string;
  /** The recommended plan: visually promoted, one per pricing table. */
  readonly highlighted: boolean;
  /** Route the plan CTA sends the visitor to. */
  readonly ctaRoute: readonly string[];
}

export interface LandingFaq {
  readonly id: string;
  readonly questionKey: string;
  readonly answerKey: string;
}

export interface LandingFooterLink {
  readonly id: string;
  readonly labelKey: string;
  /** In-app route; external/anchor links use `anchor` instead. */
  readonly route?: readonly string[];
  readonly anchor?: string;
}

export interface LandingFooterColumn {
  readonly id: string;
  readonly titleKey: string;
  readonly links: readonly LandingFooterLink[];
}
