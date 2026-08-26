export type ColorPaletteResponse = Record<string, Array<Record<string, string>>>;

export interface ColorShadeOption {
  value: string;
  familyKey: string;
  shadeKey: string;
  hex: string;
  label: string;
  searchText: string;
}

export interface ColorFamilyGroup {
  key: string;
  label: string;
  items: ColorShadeOption[];
}
