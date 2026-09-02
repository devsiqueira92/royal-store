import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductFooterComponent } from './product-footer.component';

describe('ProductFooterComponent', () => {
  let component: ProductFooterComponent;
  let fixture: ComponentFixture<ProductFooterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductFooterComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductFooterComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
