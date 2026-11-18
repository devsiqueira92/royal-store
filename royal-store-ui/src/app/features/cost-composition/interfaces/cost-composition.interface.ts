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
