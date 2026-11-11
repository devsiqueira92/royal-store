import { FulfillmentMode } from './checkout.interface';

export interface GetSalesListParams {
  sellerName?: string;
  customerName?: string;
  /** ISO date (yyyy-MM-dd) — DateOnly on the API side. */
  soldFrom?: string;
  /** ISO date (yyyy-MM-dd) — DateOnly on the API side. */
  soldTo?: string;
  fulfillmentMode?: FulfillmentMode;
}
