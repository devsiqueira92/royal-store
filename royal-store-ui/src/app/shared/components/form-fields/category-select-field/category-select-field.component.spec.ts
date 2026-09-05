import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CategorySelectFieldComponent } from './category-select-field.component';

describe('CategorySelectFieldComponent', () => {
  let component: CategorySelectFieldComponent;
  let fixture: ComponentFixture<CategorySelectFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategorySelectFieldComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CategorySelectFieldComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
