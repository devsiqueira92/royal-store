import { LabelProduct } from '../../interfaces/label-print.interface';

/**
 * Sample catalog used to demo the label sheet preview on its own, without
 * wiring it to a real product list. Repeated past the base catalog so the
 * default preview spans more than one physical page (> 80 labels).
 */
const BASE_MOCK_PRODUCTS: Omit<LabelProduct, 'id' | 'barcode'>[] = [
  { name: 'Camiseta Básica Branca P', price: 39.9 },
  { name: 'Camiseta Básica Preta M', price: 39.9 },
  { name: 'Calça Jeans Slim Azul 42', price: 129.9 },
  { name: 'Vestido Envelope Linho', price: 189.9 },
  { name: 'Jaqueta Corta-Vento Verde', price: 219.0 },
  { name: 'Short Moletom Cinza G', price: 69.9 },
  { name: 'Meia Cano Alto (par)', price: 14.9 },
  { name: 'Boné Aba Curva Bege', price: 59.9 },
  { name: 'Cinto Couro Marrom 100cm', price: 79.9 },
  { name: 'Tênis Casual Branco 40', price: 249.9 },
  { name: 'Óculos de Sol Aviador', price: 99.9 },
  { name: 'Mochila Impermeável Preta', price: 159.9 },
];

function toBarcode(index: number): string {
  return `789${(1000000000 + index).toString().slice(-10)}`;
}

export const MOCK_LABEL_PRODUCTS: LabelProduct[] = Array.from(
  { length: 90 },
  (_, index) => {
    const base = BASE_MOCK_PRODUCTS[index % BASE_MOCK_PRODUCTS.length];
    return {
      id: `mock-${index + 1}`,
      name: base.name,
      price: base.price,
      barcode: toBarcode(index + 1),
    };
  },
);
