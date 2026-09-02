import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductLogisticsComponent } from './product-logistics.component';

describe('ProductLogisticsComponent', () => {
  let component: ProductLogisticsComponent;
  let fixture: ComponentFixture<ProductLogisticsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductLogisticsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductLogisticsComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
