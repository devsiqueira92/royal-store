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
  /** Pre-configuration the label started from, if any. */
  sourceTemplateId: string | null;
  configuration: string;
  isDefault: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

/** A saved label as `api/labels` returns it; identified by `template.id`. */
export interface Label {
  format: LabelFormat;
  template: LabelTemplate;
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
 * What the designer persists: a new label, named by the user, built from a pre-configuration.
 * Both ids are `null` (new rows); `sourceTemplateId` records which pre-configuration it started from.
 * `configuration` is the JSON string of a `LabelTemplateConfiguration`.
 * Body of `POST api/labels` and `PUT api/labels/{id}` (on update, send the stored ids).
 */
export interface SaveLabelTemplateRequest {
  format: Omit<LabelFormat, 'isSystem' | 'id'> & { id: string | null };
  template: Pick<
    LabelTemplate,
    'name' | 'configuration' | 'isDefault' | 'isActive' | 'sourceTemplateId'
  > & {
    id: string | null;
    labelFormatId: string | null;
  };
}

export type CreateLabelRequest = SaveLabelTemplateRequest;
export type UpdateLabelRequest = SaveLabelTemplateRequest;

/** Controls above the measurements: the label's own name and the pre-configuration loader. */
export interface LabelDesignerSelectionControls {
  name: FormControl<string | null>;
  labelTypeId: FormControl<string | null>;
}

export const LABEL_NAME_MAX_LENGTH = 100;

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
