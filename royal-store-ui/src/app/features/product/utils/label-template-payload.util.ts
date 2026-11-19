import {
  LabelElement,
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

/**
 * Builds the request for the label type being edited: the format with the
 * measurements from the left panel, and the template with the editor's elements.
 */
export const buildSaveLabelTemplateRequest = (
  labelType: LabelType,
  spec: LabelSheetSpec,
  elements: readonly LabelElement[],
): SaveLabelTemplateRequest => {
  const configuration: LabelTemplateConfiguration = {
    version: CONFIGURATION_VERSION,
    elements: elements.map(toStoredElement),
  };

  return {
    format: {
      id: labelType.format.id,
      name: labelType.format.name,
      paperType: labelType.format.paperType,
      isActive: labelType.format.isActive,
      ...spec,
    },
    template: {
      id: labelType.id,
      name: labelType.name,
      labelFormatId: labelType.format.id,
      configuration: JSON.stringify(configuration),
      isDefault: labelType.isDefault,
      isActive: true,
    },
  };
};
