export interface UpdateProductRequest {
  categoryId: string;
  name: string;
  description?: string;
  unitOfMeasure: string;
  ncm?: string;
  cest?: string;
  origin: number;
}
