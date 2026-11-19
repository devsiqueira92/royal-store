import { FormControl } from '@angular/forms';

/** Mirrors the API's `LabelPaperType`. */
export enum LabelPaperType {
  Sheet = 0,
  Roll = 1,
}

/** Physical sheet/roll spec. All sizes in millimetres. Mirrors the API's `LabelFormat`. */
export interface LabelFormat {
  id: string;
  name: string;
  paperType: LabelPaperType;
  paperWidth: number;
  paperHeight: number;
  labelWidth: number;
  labelHeight: number;
  columns: number;
  horizontalSpacing: number;
  verticalSpacing: number;
  marginTop: number;
  marginRight: number;
  marginBottom: number;
  marginLeft: number;
  isSystem: boolean;
  isActive: boolean;
}

export type LabelElementType = 'text' | 'barcode';
export type LabelElementAlignment = 'left' | 'center' | 'right';
export type LabelElementFontWeight = 'normal' | 'bold';

/** One element drawn inside a label. Position/size in mm, relative to the label's top-left corner. */
export interface LabelElement {
  type: LabelElementType;
  field: string;
  x: number;
  y: number;
  width: number;
  height: number;
  fontSize?: number;
  fontWeight?: LabelElementFontWeight;
  alignment?: LabelElementAlignment;
  /** `currency` for text, symbology (e.g. `CODE128`) for barcodes. */
  format?: string;
  showValue?: boolean;
}

/** Parsed form of `LabelTemplate.configuration`. */
export interface LabelTemplateConfiguration {
  version: number;
  elements: LabelElement[];
}

/** Mirrors the API's `LabelTemplate`; `configuration` is the raw JSON string. */
export interface LabelTemplate {
  id: string;
  name: string;
  labelFormatId: string;
  configuration: string;
  isDefault: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

/** A template joined with its format and parsed configuration — one dropdown entry. */
export interface LabelType {
  id: string;
  name: string;
  isDefault: boolean;
  format: LabelFormat;
  configuration: LabelTemplateConfiguration;
}

/**
 * What the designer persists: the (possibly edited) sheet format and the template
 * whose `configuration` is the JSON string of a `LabelTemplateConfiguration`.
 * Endpoint still to be defined.
 */
export interface SaveLabelTemplateRequest {
  format: Omit<LabelFormat, 'isSystem'>;
  template: Pick<
    LabelTemplate,
    'id' | 'name' | 'labelFormatId' | 'configuration' | 'isDefault' | 'isActive'
  >;
}

/** The editable numeric part of a `LabelFormat`. */
export type LabelSheetSpec = Pick<
  LabelFormat,
  | 'paperWidth'
  | 'paperHeight'
  | 'labelWidth'
  | 'labelHeight'
  | 'columns'
  | 'horizontalSpacing'
  | 'verticalSpacing'
  | 'marginTop'
  | 'marginRight'
  | 'marginBottom'
  | 'marginLeft'
>;

export type LabelSheetFormControls = {
  [K in keyof LabelSheetSpec]: FormControl<number | null>;
};

export interface LabelSheetFieldDefinition {
  controlName: keyof LabelSheetSpec;
  labelKey: string;
  /** Whole numbers only (e.g. columns). */
  integer?: boolean;
}

/** One label's rectangle on the sheet, in mm. */
export interface LabelCell {
  row: number;
  column: number;
  x: number;
  y: number;
  width: number;
  height: number;
  /** Falls (partly) outside the printable area. */
  overflows: boolean;
}

export interface LabelSheetLayout {
  spec: LabelSheetSpec;
  rows: number;
  cells: LabelCell[];
  /** Width taken by margins + labels + spacing; above `paperWidth` means the columns do not fit. */
  usedWidth: number;
  overflowsWidth: boolean;
}
