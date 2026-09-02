export interface IbptTax {
  id: string;
  code: string;
  exception?: string | null;
  type: number;
  description: string;
  nationalRate: number;
  federalRate: number;
  importedFederalRate: number;
  stateRate: number;
  municipalRate: number;
  effectiveStart: string;
  effectiveEnd: string;
  isActive: boolean;
}
