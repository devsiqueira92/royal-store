import { RegisterSaleResponse } from '../interfaces/register-sale-response.interface';
import { SaleLineItem } from '../interfaces/sale-line-item.interface';
import { EMPTY_SALE_CART } from '../interfaces/sale-cart.interface';
import { SaleActions } from './sale.actions';
import { initialSaleState, saleFeature } from './sale.reducer';

const item: SaleLineItem = {
  variantId: 'v1',
  productId: 'p1',
  name: 'Ring',
  variantLabel: '',
  unitPrice: 50,
  quantity: 2,
  origin: '0',
};

const response = { saleId: 's1' } as RegisterSaleResponse;

describe('sale reducer — cart', () => {
  const reduce = saleFeature.reducer;

  it('keeps items and discount until the sale registers', () => {
    let state = reduce(initialSaleState, SaleActions.setCartItems({ items: [item] }));
    state = reduce(state, SaleActions.setCartDiscount({ value: 10, mode: 'percent' }));

    expect(state.cart).toEqual({
      items: [item],
      discountValue: 10,
      discountMode: 'percent',
    });

    state = reduce(state, SaleActions.registerSaleFailure({ error: 'boom' }));
    expect(state.cart.items).toEqual([item]);

    state = reduce(state, SaleActions.registerSaleSuccess({ response }));
    expect(state.cart).toEqual(EMPTY_SALE_CART);
    expect(state.lastRegisterSaleResult?.saleId).toBe('s1');
  });

  it('clears on demand', () => {
    const filled = reduce(initialSaleState, SaleActions.setCartItems({ items: [item] }));

    expect(reduce(filled, SaleActions.clearCart()).cart).toEqual(EMPTY_SALE_CART);
  });
});
