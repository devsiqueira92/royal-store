import {
  LandingFaq,
  LandingFeature,
  LandingFooterColumn,
  LandingNavLink,
  LandingPlan,
  LandingShowcaseTab,
  LandingStat,
} from '../models/landing.model';

/** Section ids used for both the DOM anchors and the navbar links. */
export const LANDING_SECTION_IDS = {
  features: 'funcionalidades',
  showcase: 'como-funciona',
  pricing: 'planos',
  faq: 'faq',
} as const;

export const LANDING_NAV_LINKS: readonly LandingNavLink[] = [
  { id: LANDING_SECTION_IDS.features, labelKey: 'landing.nav.features' },
  { id: LANDING_SECTION_IDS.showcase, labelKey: 'landing.nav.howItWorks' },
  { id: LANDING_SECTION_IDS.pricing, labelKey: 'landing.nav.pricing' },
  { id: LANDING_SECTION_IDS.faq, labelKey: 'landing.nav.faq' },
];

export const LANDING_STATS: readonly LandingStat[] = [
  {
    id: 'stores',
    icon: 'pi pi-shop',
    valueKey: 'landing.stats.stores.value',
    labelKey: 'landing.stats.stores.label',
  },
  {
    id: 'uptime',
    icon: 'pi pi-shield',
    valueKey: 'landing.stats.uptime.value',
    labelKey: 'landing.stats.uptime.label',
  },
  {
    id: 'hours',
    icon: 'pi pi-clock',
    valueKey: 'landing.stats.hours.value',
    labelKey: 'landing.stats.hours.label',
  },
  {
    id: 'rating',
    icon: 'pi pi-star-fill',
    valueKey: 'landing.stats.rating.value',
    labelKey: 'landing.stats.rating.label',
  },
];

export const LANDING_FEATURES: readonly LandingFeature[] = [
  {
    id: 'pos',
    icon: 'pi pi-barcode',
    span: 'hero',
    tagKey: 'landing.features.pos.tag',
    titleKey: 'landing.features.pos.title',
    descriptionKey: 'landing.features.pos.description',
    bulletKeys: [
      'landing.features.pos.bullets.b1',
      'landing.features.pos.bullets.b2',
      'landing.features.pos.bullets.b3',
    ],
  },
  {
    id: 'inventory',
    icon: 'pi pi-box',
    span: 'wide',
    titleKey: 'landing.features.inventory.title',
    descriptionKey: 'landing.features.inventory.description',
  },
  {
    id: 'finance',
    icon: 'pi pi-chart-line',
    span: 'normal',
    titleKey: 'landing.features.finance.title',
    descriptionKey: 'landing.features.finance.description',
  },
  {
    id: 'invoice',
    icon: 'pi pi-file-check',
    span: 'normal',
    titleKey: 'landing.features.invoice.title',
    descriptionKey: 'landing.features.invoice.description',
  },
  {
    id: 'loyalty',
    icon: 'pi pi-users',
    span: 'wide',
    titleKey: 'landing.features.loyalty.title',
    descriptionKey: 'landing.features.loyalty.description',
  },
  {
    id: 'cashShift',
    icon: 'pi pi-wallet',
    span: 'wide',
    titleKey: 'landing.features.cashShift.title',
    descriptionKey: 'landing.features.cashShift.description',
  },
];

export const LANDING_SHOWCASE_TABS: readonly LandingShowcaseTab[] = [
  {
    id: 'pos',
    icon: 'pi pi-barcode',
    variant: 'pos',
    labelKey: 'landing.showcase.pos.label',
    titleKey: 'landing.showcase.pos.title',
    descriptionKey: 'landing.showcase.pos.description',
    mockupAltKey: 'landing.showcase.pos.mockupAlt',
    bulletKeys: [
      'landing.showcase.pos.bullets.b1',
      'landing.showcase.pos.bullets.b2',
      'landing.showcase.pos.bullets.b3',
    ],
  },
  {
    id: 'inventory',
    icon: 'pi pi-box',
    variant: 'inventory',
    labelKey: 'landing.showcase.inventory.label',
    titleKey: 'landing.showcase.inventory.title',
    descriptionKey: 'landing.showcase.inventory.description',
    mockupAltKey: 'landing.showcase.inventory.mockupAlt',
    bulletKeys: [
      'landing.showcase.inventory.bullets.b1',
      'landing.showcase.inventory.bullets.b2',
      'landing.showcase.inventory.bullets.b3',
    ],
  },
  {
    id: 'finance',
    icon: 'pi pi-chart-bar',
    variant: 'finance',
    labelKey: 'landing.showcase.finance.label',
    titleKey: 'landing.showcase.finance.title',
    descriptionKey: 'landing.showcase.finance.description',
    mockupAltKey: 'landing.showcase.finance.mockupAlt',
    bulletKeys: [
      'landing.showcase.finance.bullets.b1',
      'landing.showcase.finance.bullets.b2',
      'landing.showcase.finance.bullets.b3',
    ],
  },
];

export const LANDING_PLANS: readonly LandingPlan[] = [
  {
    id: 'essential',
    nameKey: 'landing.pricing.essential.name',
    descriptionKey: 'landing.pricing.essential.description',
    monthlyPrice: 79,
    annualPrice: 63,
    featureKeys: [
      'landing.pricing.essential.features.f1',
      'landing.pricing.essential.features.f2',
      'landing.pricing.essential.features.f3',
      'landing.pricing.essential.features.f4',
      'landing.pricing.essential.features.f5',
    ],
    ctaKey: 'landing.pricing.essential.cta',
    highlighted: false,
    ctaRoute: ['/auth/register'],
  },
  {
    id: 'professional',
    nameKey: 'landing.pricing.professional.name',
    descriptionKey: 'landing.pricing.professional.description',
    monthlyPrice: 149,
    annualPrice: 119,
    featureKeys: [
      'landing.pricing.professional.features.f1',
      'landing.pricing.professional.features.f2',
      'landing.pricing.professional.features.f3',
      'landing.pricing.professional.features.f4',
      'landing.pricing.professional.features.f5',
      'landing.pricing.professional.features.f6',
    ],
    ctaKey: 'landing.pricing.professional.cta',
    highlighted: true,
    ctaRoute: ['/auth/register'],
  },
  {
    id: 'enterprise',
    nameKey: 'landing.pricing.enterprise.name',
    descriptionKey: 'landing.pricing.enterprise.description',
    monthlyPrice: null,
    annualPrice: null,
    featureKeys: [
      'landing.pricing.enterprise.features.f1',
      'landing.pricing.enterprise.features.f2',
      'landing.pricing.enterprise.features.f3',
      'landing.pricing.enterprise.features.f4',
      'landing.pricing.enterprise.features.f5',
    ],
    ctaKey: 'landing.pricing.enterprise.cta',
    highlighted: false,
    ctaRoute: ['/auth/register'],
  },
];

export const LANDING_FAQS: readonly LandingFaq[] = [
  {
    id: 'migration',
    questionKey: 'landing.faq.migration.question',
    answerKey: 'landing.faq.migration.answer',
  },
  {
    id: 'trial',
    questionKey: 'landing.faq.trial.question',
    answerKey: 'landing.faq.trial.answer',
  },
  {
    id: 'cancel',
    questionKey: 'landing.faq.cancel.question',
    answerKey: 'landing.faq.cancel.answer',
  },
  {
    id: 'support',
    questionKey: 'landing.faq.support.question',
    answerKey: 'landing.faq.support.answer',
  },
  {
    id: 'hardware',
    questionKey: 'landing.faq.hardware.question',
    answerKey: 'landing.faq.hardware.answer',
  },
  {
    id: 'offline',
    questionKey: 'landing.faq.offline.question',
    answerKey: 'landing.faq.offline.answer',
  },
  {
    id: 'invoice',
    questionKey: 'landing.faq.invoice.question',
    answerKey: 'landing.faq.invoice.answer',
  },
];

export const LANDING_FOOTER_COLUMNS: readonly LandingFooterColumn[] = [
  {
    id: 'product',
    titleKey: 'landing.footer.product.title',
    links: [
      {
        id: 'features',
        labelKey: 'landing.footer.product.features',
        anchor: LANDING_SECTION_IDS.features,
      },
      {
        id: 'showcase',
        labelKey: 'landing.footer.product.showcase',
        anchor: LANDING_SECTION_IDS.showcase,
      },
      {
        id: 'pricing',
        labelKey: 'landing.footer.product.pricing',
        anchor: LANDING_SECTION_IDS.pricing,
      },
      { id: 'login', labelKey: 'landing.footer.product.login', route: ['/auth/login'] },
    ],
  },
  {
    id: 'resources',
    titleKey: 'landing.footer.resources.title',
    links: [
      { id: 'faq', labelKey: 'landing.footer.resources.faq', anchor: LANDING_SECTION_IDS.faq },
      {
        id: 'migration',
        labelKey: 'landing.footer.resources.migration',
        anchor: LANDING_SECTION_IDS.faq,
      },
      { id: 'register', labelKey: 'landing.footer.resources.register', route: ['/auth/register'] },
    ],
  },
  {
    id: 'company',
    titleKey: 'landing.footer.company.title',
    links: [
      { id: 'about', labelKey: 'landing.footer.company.about', anchor: 'hero' },
      { id: 'contact', labelKey: 'landing.footer.company.contact', anchor: 'cta-final' },
    ],
  },
];
