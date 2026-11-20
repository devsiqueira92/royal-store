import {
  Label,
  LabelElement,
  LabelPaperType,
  LabelSheetSpec,
  LabelTemplateConfiguration,
  LabelType,
  SaveLabelTemplateRequest,
} from '../interfaces/label-format.interface';

const CONFIGURATION_VERSION = 1;

const round2 = (value: number): number => Math.round(value * 100) / 100;

/** Rounds the geometry and drops unset optional keys so the stored JSON stays clean. */
const toStoredElement = (element: LabelElement): LabelElement => {
  const stored: LabelElement = {
    type: element.type,
    field: element.field,
    x: round2(element.x),
    y: round2(element.y),
    width: round2(element.width),
    height: round2(element.height),
  };
  if (element.fontSize != null) stored.fontSize = element.fontSize;
  if (element.fontWeight != null) stored.fontWeight = element.fontWeight;
  if (element.alignment != null) stored.alignment = element.alignment;
  if (element.format != null) stored.format = element.format;
  if (element.showValue != null) stored.showValue = element.showValue;
  return stored;
};

/** Identity and flags of the label being saved; everything the designer does not edit. */
export interface LabelSaveContext {
  /** `null` → new label (POST). */
  formatId: string | null;
  templateId: string | null;
  paperType: LabelPaperType;
  sourceTemplateId: string | null;
  isDefault: boolean;
  isActive: boolean;
}

/** A new label started from a pre-configuration. */
export const saveContextFromPreset = (preset: LabelType): LabelSaveContext => ({
  formatId: null,
  templateId: null,
  paperType: preset.format.paperType,
  sourceTemplateId: preset.id,
  isDefault: false,
  isActive: true,
});

/** A stored label being edited; keeps its ids and flags. */
export const saveContextFromLabel = (label: Label): LabelSaveContext => ({
  formatId: label.format.id,
  templateId: label.template.id,
  paperType: label.format.paperType,
  sourceTemplateId: label.template.sourceTemplateId,
  isDefault: label.template.isDefault,
  isActive: label.template.isActive,
});

/** Parses a stored `configuration`; an unreadable one yields no elements. */
export const parseLabelConfiguration = (configuration: string): LabelElement[] => {
  try {
    const parsed = JSON.parse(configuration) as Partial<LabelTemplateConfiguration>;
    return Array.isArray(parsed.elements) ? parsed.elements : [];
  } catch {
    return [];
  }
};

/**
 * Builds the `{ format, template }` body for the label named `name`: the format with
 * the measurements from the left panel and the template with the editor's elements.
 */
export const buildSaveLabelTemplateRequest = (
  context: LabelSaveContext,
  name: string,
  spec: LabelSheetSpec,
  elements: readonly LabelElement[],
): SaveLabelTemplateRequest => {
  const configuration: LabelTemplateConfiguration = {
    version: CONFIGURATION_VERSION,
    elements: elements.map(toStoredElement),
  };

  return {
    format: {
      id: context.formatId,
      name,
      paperType: context.paperType,
      isActive: context.isActive,
      ...spec,
    },
    template: {
      id: context.templateId,
      name,
      labelFormatId: context.formatId,
      sourceTemplateId: context.sourceTemplateId,
      configuration: JSON.stringify(configuration),
      isDefault: context.isDefault,
      isActive: context.isActive,
    },
  };
};
