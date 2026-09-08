import {
  loadLoyaltyTiers,
  loadLoyaltyTier,
  createLoyaltyTier,
  updateLoyaltyTier,
  deleteLoyaltyTier,
  activateLoyaltyTier,
  deactivateLoyaltyTier,
} from './loyalty-tier.effects';

export const loyaltyTierStore = {
  loadLoyaltyTiers,
  loadLoyaltyTier,
  createLoyaltyTier,
  updateLoyaltyTier,
  deleteLoyaltyTier,
  activateLoyaltyTier,
  deactivateLoyaltyTier,
};
