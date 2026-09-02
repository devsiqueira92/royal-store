import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductTaxesComponent } from './product-taxes.component';

describe('ProductTaxesComponent', () => {
  let component: ProductTaxesComponent;
  let fixture: ComponentFixture<ProductTaxesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductTaxesComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductTaxesComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
