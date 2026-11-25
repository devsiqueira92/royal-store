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

/** PUT api/price-lists/{id}: name and rule in one request. Rule fields both null = no rule. */
export interface UpdatePriceListRequest {
  name: string;
  basePriceListId: string | null;
  discountPercentage: number | null;
}
