export interface CreateLoyaltyTierRequest {
  name: string;
  cashbackPercentage: number;
  cashbackValidityDays: number;
}

// Name is fixed once the tier exists — the API only accepts the cashback rules on update.
export interface UpdateLoyaltyTierRequest {
  cashbackPercentage: number;
  cashbackValidityDays: number;
}
