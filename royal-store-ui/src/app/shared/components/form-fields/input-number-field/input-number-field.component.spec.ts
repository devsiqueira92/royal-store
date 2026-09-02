import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InputNumberFieldComponent } from './input-number-field.component';

describe('InputNumberFieldComponent', () => {
  let component: InputNumberFieldComponent;
  let fixture: ComponentFixture<InputNumberFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InputNumberFieldComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(InputNumberFieldComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
