import {
  loadLoyaltyTiers,
  loadLoyaltyTierOptions,
  loadLoyaltyTier,
  createLoyaltyTier,
  updateLoyaltyTier,
  deleteLoyaltyTier,
  activateLoyaltyTier,
  deactivateLoyaltyTier,
} from './loyalty-tier.effects';

export const loyaltyTierStore = {
  loadLoyaltyTiers,
  loadLoyaltyTierOptions,
  loadLoyaltyTier,
  createLoyaltyTier,
  updateLoyaltyTier,
  deleteLoyaltyTier,
  activateLoyaltyTier,
  deactivateLoyaltyTier,
};
