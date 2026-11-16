export interface PriceList {
  id: string;
  name: string;
  isDefault: boolean;
  isActive: boolean;
  productCount: number;
  discountPercentage: number | null;
  basePriceListId: string | null;
  basePriceListName: string | null;
  dependentListCount: number;
}

export interface CreatePriceListRequest {
  name: string;
  basePriceListId?: string | null;
  discountPercentage?: number | null;
}

export interface RenamePriceListRequest {
  name: string;
}

export interface SetPriceListRuleRequest {
  basePriceListId: string;
  discountPercentage: number;
}
