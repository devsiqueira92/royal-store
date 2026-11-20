import {
  LabelFormat,
  LabelPaperType,
  LabelTemplate,
} from '../interfaces/label-format.interface';

/**
 * Stand-in data until the label format/template endpoints exist.
 * Remove together with the `of(...)` calls in `LabelFormatService`.
 */
const SAMPLE_CONFIGURATION = JSON.stringify({
  version: 1,
  elements: [
    {
      type: 'text',
      field: 'product.name',
      x: 2,
      y: 2,
      width: 46,
      height: 5,
      fontSize: 9,
      fontWeight: 'bold',
      alignment: 'center',
    },
    {
      type: 'text',
      field: 'product.price',
      x: 2,
      y: 9,
      width: 46,
      height: 7,
      fontSize: 14,
      fontWeight: 'bold',
      alignment: 'center',
      format: 'currency',
    },
    {
      type: 'barcode',
      field: 'product.barcode',
      x: 5,
      y: 18,
      width: 40,
      height: 6,
      format: 'CODE128',
      showValue: true,
    },
  ],
});

const systemFormat = (
  format: Omit<LabelFormat, 'isSystem' | 'isActive'>,
): LabelFormat => ({ ...format, isSystem: true, isActive: true });

export const MOCK_LABEL_FORMATS: LabelFormat[] = [
  systemFormat({
    id: 'format-a4-3x7',
    name: 'A4 — 3 × 7 (63,5 × 38,1 mm)',
    paperType: LabelPaperType.Sheet,
    paperWidth: 210,
    paperHeight: 297,
    labelWidth: 63.5,
    labelHeight: 38.1,
    columns: 3,
    horizontalSpacing: 2.5,
    verticalSpacing: 0,
    marginTop: 15.15,
    marginRight: 7.25,
    marginBottom: 15.15,
    marginLeft: 7.25,
  }),
  systemFormat({
    id: 'format-letter-6',
    name: 'Carta — 6 colunas (33 × 18 mm)',
    paperType: LabelPaperType.Sheet,
    paperWidth: 216,
    paperHeight: 279,
    labelWidth: 33,
    labelHeight: 18,
    columns: 6,
    horizontalSpacing: 2,
    verticalSpacing: 0,
    marginTop: 12.7,
    marginRight: 4,
    marginBottom: 12.7,
    marginLeft: 4,
  }),
  systemFormat({
    id: 'format-roll-50x30',
    name: 'Rolo térmico 50 × 30 mm',
    paperType: LabelPaperType.Roll,
    paperWidth: 50,
    paperHeight: 30,
    labelWidth: 50,
    labelHeight: 30,
    columns: 1,
    horizontalSpacing: 0,
    verticalSpacing: 0,
    marginTop: 0,
    marginRight: 0,
    marginBottom: 0,
    marginLeft: 0,
  }),
];

const template = (
  id: string,
  name: string,
  labelFormatId: string,
  isDefault = false,
): LabelTemplate => ({
  id,
  name,
  labelFormatId,
  sourceTemplateId: null,
  configuration: SAMPLE_CONFIGURATION,
  isDefault,
  isActive: true,
  createdAt: '2026-11-19T00:00:00Z',
  updatedAt: '2026-11-19T00:00:00Z',
});

export const MOCK_LABEL_TEMPLATES: LabelTemplate[] = [
  template('template-a4', 'Gôndola A4 (3 colunas)', 'format-a4-3x7', true),
  template('template-letter', 'Gôndola Carta (6 colunas)', 'format-letter-6'),
  template('template-roll', 'Rolo térmico 50 × 30', 'format-roll-50x30'),
];
