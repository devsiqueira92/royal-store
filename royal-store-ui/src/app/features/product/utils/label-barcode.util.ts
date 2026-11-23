import JsBarcode from 'jsbarcode';

/**
 * One dot of a 203 dpi thermal printer, the coarsest common label printer.
 * Bars sized in whole dots print with even widths there, and stay exact on
 * 300/600 dpi lasers; fractional sizes get rounded unevenly and stop scanning.
 */
export const BARCODE_DOT_MM = 25.4 / 203;

/** Narrowest bar (module) that still scans reliably: 2 dots ≈ 0.25 mm. */
export const MIN_BARCODE_MODULE_MM = 2 * BARCODE_DOT_MM;

/** Font size (pt) of the digits under the bars when the element has none. */
export const DEFAULT_BARCODE_VALUE_FONT_PT = 6;

export const BARCODE_FORMAT = 'CODE128';

export interface BarcodeGeometry {
  /** Narrow-bar count of the encoded value. */
  modules: number;
  /** Width of one module, a whole number of printer dots when it fits. */
  moduleMm: number;
  /** Width actually taken by the bars (≤ the element's width). */
  widthMm: number;
  readable: boolean;
  /** Element width needed for a readable code. */
  minWidthMm: number;
}

interface EncodedTarget {
  encodings?: { data: string }[];
}

/** Modules of `value` in CODE128; `null` when it cannot be encoded. */
export const barcodeModuleCount = (value: string): number | null => {
  if (!value) {
    return null;
  }
  const target: EncodedTarget = {};
  try {
    JsBarcode(target, value, { format: BARCODE_FORMAT });
  } catch {
    return null;
  }
  const modules = (target.encodings ?? []).reduce((sum, e) => sum + e.data.length, 0);
  return modules > 0 ? modules : null;
};

/**
 * Sizes the bars for an element `availableMm` wide: the widest module that is a
 * whole number of printer dots and still fits. When not even one dot per module
 * fits, the bars fill the element and are flagged as unreadable.
 */
export const computeBarcodeGeometry = (
  value: string,
  availableMm: number,
): BarcodeGeometry | null => {
  const modules = barcodeModuleCount(value);
  if (!modules || availableMm <= 0) {
    return null;
  }
  const dots = Math.floor(availableMm / modules / BARCODE_DOT_MM);
  const moduleMm = dots >= 1 ? dots * BARCODE_DOT_MM : availableMm / modules;
  return {
    modules,
    moduleMm,
    widthMm: modules * moduleMm,
    readable: moduleMm >= MIN_BARCODE_MODULE_MM - 1e-6,
    minWidthMm: Math.ceil(modules * MIN_BARCODE_MODULE_MM * 10) / 10,
  };
};
