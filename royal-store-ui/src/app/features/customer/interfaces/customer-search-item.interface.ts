/** Wire shape of `CustomerSearchItemResponse`: an active customer as the checkout lists it. */
export interface CustomerSearchItem {
  id: string;
  name: string;
  loyaltyTierId: string;
  loyaltyTierName: string;
  /** Whole percentage ("4" means 4%). */
  loyaltyTierCashbackPercentage: number;
}
