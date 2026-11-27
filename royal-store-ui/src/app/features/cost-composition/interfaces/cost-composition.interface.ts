export interface CostCompositionItem {
  id: string;
  description: string;
  value: number;
}

export interface CostComposition {
  id: string;
  name: string;
  createdAt: string;
  totalValue: number;
  items: CostCompositionItem[];
}

/** One row of GET api/cost-compositions: no lines, only their count. */
export interface CostCompositionSummary {
  id: string;
  /** How many products use the list: a cost composition is reusable. */
  productsCount: number;
  name: string;
  totalValue: number;
  numberItems: number;
}

/** `productsCount` is not in the full response: a new list has none, an edit keeps the known count. */
export const toCostCompositionSummary = (
  c: CostComposition,
  productsCount = 0,
): CostCompositionSummary => ({
  id: c.id,
  productsCount,
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
