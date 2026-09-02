/**
 * Mocked tax profile templates. Replace with a real endpoint once available.
 * Each template pre-fills the Advanced Fiscal Settings (CFOP + CST codes).
 */
export interface TaxProfileFiscalDefaults {
  cest: string;
  defaultCfop: string;
  icmsCst: string;
  pisCofinsCst: string;
  ivaCst: string;
  isSelectiveTax: boolean;
}

export interface TaxProfileTemplate {
  code: string;
  label: string;
  helperText: string;
  itemType: 0 | 1;
  fiscal: TaxProfileFiscalDefaults;
}

export const PRODUCT_TAX_PROFILE_TEMPLATES: TaxProfileTemplate[] = [
  {
    code: 'retail-simples',
    label: 'Standard Retail — Simples Nacional',
    helperText:
      'Retail sale to final consumer, no tax substitution. Covers most clothing and accessories.',
    itemType: 0,
    fiscal: {
      cest: '00.000.00',
      defaultCfop: '5102',
      icmsCst: '102',
      pisCofinsCst: '49',
      ivaCst: '000',
      isSelectiveTax: false,
    },
  },
  {
    code: 'retail-st-simples',
    label: 'Retail with ICMS-ST — Simples Nacional',
    helperText:
      'Retail sale of goods already taxed by substitution upstream. ICMS was collected by the supplier.',
    itemType: 0,
    fiscal: {
      cest: '28.064.00',
      defaultCfop: '5405',
      icmsCst: '500',
      pisCofinsCst: '49',
      ivaCst: '000',
      isSelectiveTax: false,
    },
  },
  {
    code: 'resale-normal',
    label: 'Resale — Normal Regime',
    helperText:
      'Resale of purchased goods under the normal (non-Simples) regime, with full ICMS/PIS/COFINS debit.',
    itemType: 0,
    fiscal: {
      cest: '00.000.00',
      defaultCfop: '5102',
      icmsCst: '00',
      pisCofinsCst: '01',
      ivaCst: '000',
      isSelectiveTax: false,
    },
  },
];

export const SERVICE_TAX_PROFILE_TEMPLATES: TaxProfileTemplate[] = [
  {
    code: 'service-iss',
    label: 'Services — ISS municipal',
    helperText:
      'Service invoice (NFS-e) with ISS withheld by your municipality. No ICMS applies.',
    itemType: 1,
    fiscal: {
      cest: '',
      defaultCfop: '5933',
      icmsCst: '',
      pisCofinsCst: '49',
      ivaCst: '000',
      isSelectiveTax: false,
    },
  },
  {
    code: 'service-iss-retained',
    label: 'Services — ISS retained at source',
    helperText:
      'Service invoice where the client (taker) is responsible for retaining and paying the ISS.',
    itemType: 1,
    fiscal: {
      cest: '',
      defaultCfop: '5933',
      icmsCst: '',
      pisCofinsCst: '50',
      ivaCst: '000',
      isSelectiveTax: false,
    },
  },
];

export function taxProfileTemplatesFor(itemType: number): TaxProfileTemplate[] {
  return itemType === 1
    ? SERVICE_TAX_PROFILE_TEMPLATES
    : PRODUCT_TAX_PROFILE_TEMPLATES;
}

export function findTaxProfileTemplate(
  code: string | null | undefined,
): TaxProfileTemplate | undefined {
  if (!code) {
    return undefined;
  }
  return [
    ...PRODUCT_TAX_PROFILE_TEMPLATES,
    ...SERVICE_TAX_PROFILE_TEMPLATES,
  ].find(template => template.code === code);
}
