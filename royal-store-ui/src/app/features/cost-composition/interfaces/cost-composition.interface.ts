export interface CostCompositionItem {
  id: string;
  description: string;
  value: number;
}

export interface CostComposition {
  id: string;
  name: string;
  productId: string | null;
  createdAt: string;
  totalValue: number;
  items: CostCompositionItem[];
}

/** One row of GET api/cost-compositions: no lines, only their count. */
export interface CostCompositionSummary {
  id: string;
  productId: string | null;
  name: string;
  totalValue: number;
  numberItems: number;
}

export const toCostCompositionSummary = (
  c: CostComposition,
): CostCompositionSummary => ({
  id: c.id,
  productId: c.productId,
  name: c.name,
  totalValue: c.totalValue,
  numberItems: c.items.length,
});

/** `id` null = new line. */
export interface CostCompositionItemInput {
  id: string | null;
  description: string;
  value: number;
}

export interface CreateCostCompositionRequest {
  name: string;
  items: CostCompositionItemInput[];
}

export interface UpdateCostCompositionRequest {
  name: string;
  items: CostCompositionItemInput[];
}
