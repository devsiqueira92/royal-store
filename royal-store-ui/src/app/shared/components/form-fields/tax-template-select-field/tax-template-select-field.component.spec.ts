import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaxTemplateSelectFieldComponent } from './tax-template-select-field.component';

describe('TaxTemplateSelectFieldComponent', () => {
  let component: TaxTemplateSelectFieldComponent;
  let fixture: ComponentFixture<TaxTemplateSelectFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaxTemplateSelectFieldComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TaxTemplateSelectFieldComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
