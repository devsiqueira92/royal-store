import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CashShiftFormComponent } from './cash-shift-form.component';

describe('CashShiftFormComponent', () => {
  let component: CashShiftFormComponent;
  let fixture: ComponentFixture<CashShiftFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CashShiftFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CashShiftFormComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
