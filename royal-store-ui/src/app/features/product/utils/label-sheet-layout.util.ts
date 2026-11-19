import {
  LabelCell,
  LabelSheetFieldDefinition,
  LabelSheetLayout,
  LabelSheetSpec,
} from '../interfaces/label-format.interface';

/** Hard cap so a typo (e.g. label height 0.01) cannot freeze the preview. */
const MAX_CELLS = 2000;

const EPSILON = 0.0001;

export const LABEL_SHEET_FIELDS: LabelSheetFieldDefinition[] = [
  { controlName: 'paperWidth', labelKey: 'product.labelDesigner.fields.paperWidth' },
  { controlName: 'paperHeight', labelKey: 'product.labelDesigner.fields.paperHeight' },
  { controlName: 'marginTop', labelKey: 'product.labelDesigner.fields.marginTop' },
  { controlName: 'marginRight', labelKey: 'product.labelDesigner.fields.marginRight' },
  { controlName: 'marginBottom', labelKey: 'product.labelDesigner.fields.marginBottom' },
  { controlName: 'marginLeft', labelKey: 'product.labelDesigner.fields.marginLeft' },
  { controlName: 'columns', labelKey: 'product.labelDesigner.fields.columns', integer: true },
];

export const LABEL_FIELDS: LabelSheetFieldDefinition[] = [
  { controlName: 'labelWidth', labelKey: 'product.labelDesigner.fields.labelWidth' },
  { controlName: 'labelHeight', labelKey: 'product.labelDesigner.fields.labelHeight' },
  { controlName: 'horizontalSpacing', labelKey: 'product.labelDesigner.fields.horizontalSpacing' },
  { controlName: 'verticalSpacing', labelKey: 'product.labelDesigner.fields.verticalSpacing' },
];

const positive = (value: number | null | undefined): number =>
  value != null && Number.isFinite(value) && value > 0 ? value : 0;

/** Blank/invalid inputs count as 0 so the preview never breaks while the user types. */
export const toLabelSheetSpec = (
  raw: Partial<Record<keyof LabelSheetSpec, number | null>>,
): LabelSheetSpec => ({
  paperWidth: positive(raw.paperWidth),
  paperHeight: positive(raw.paperHeight),
  labelWidth: positive(raw.labelWidth),
  labelHeight: positive(raw.labelHeight),
  columns: Math.trunc(positive(raw.columns)),
  horizontalSpacing: positive(raw.horizontalSpacing),
  verticalSpacing: positive(raw.verticalSpacing),
  marginTop: positive(raw.marginTop),
  marginRight: positive(raw.marginRight),
  marginBottom: positive(raw.marginBottom),
  marginLeft: positive(raw.marginLeft),
});

/**
 * Grid placement, same formula as the backend:
 *   X = MarginLeft + Column × (LabelWidth + HorizontalSpacing)
 *   Y = MarginTop  + Row    × (LabelHeight + VerticalSpacing)
 * Rows are as many as fit between the top and bottom margins.
 */
export const computeLabelSheetLayout = (
  spec: LabelSheetSpec,
): LabelSheetLayout => {
  const {
    paperWidth,
    paperHeight,
    labelWidth,
    labelHeight,
    columns,
    horizontalSpacing,
    verticalSpacing,
    marginTop,
    marginRight,
    marginBottom,
    marginLeft,
  } = spec;

  const usedWidth =
    marginLeft +
    columns * labelWidth +
    Math.max(columns - 1, 0) * horizontalSpacing +
    marginRight;

  const printableHeight = paperHeight - marginTop - marginBottom;
  const rows =
    labelHeight > 0 && printableHeight >= labelHeight
      ? Math.floor(
          (printableHeight + verticalSpacing + EPSILON) /
            (labelHeight + verticalSpacing),
        )
      : 0;

  const rightLimit = paperWidth - marginRight + EPSILON;
  const cells: LabelCell[] = [];

  if (labelWidth > 0 && labelHeight > 0) {
    for (let row = 0; row < rows; row++) {
      for (let column = 0; column < columns; column++) {
        if (cells.length >= MAX_CELLS) {
          break;
        }
        const x = marginLeft + column * (labelWidth + horizontalSpacing);
        const y = marginTop + row * (labelHeight + verticalSpacing);
        cells.push({
          row,
          column,
          x,
          y,
          width: labelWidth,
          height: labelHeight,
          overflows: x + labelWidth > rightLimit,
        });
      }
    }
  }

  return {
    spec,
    rows,
    cells,
    usedWidth,
    overflowsWidth: usedWidth > paperWidth + EPSILON,
  };
};
