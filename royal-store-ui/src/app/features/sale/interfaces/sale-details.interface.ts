import { SaleItemDetails } from './sale-item-details.interface';

export interface SaleDetails {
  id: string;
  subtotal: number;
  discountAmount: number;
  loyaltyDiscountApplied: number;
  paymentDiscountApplied: number;
  taxAmount: number;
  totalAmount: number;
  cashbackApplied: number;
  fulfillmentMode: string;
  sellerName: string | null;
  customerName: string | null;
  customerLoyaltyTierName: string | null;
  soldAt: string;
  items: SaleItemDetails[];
}
