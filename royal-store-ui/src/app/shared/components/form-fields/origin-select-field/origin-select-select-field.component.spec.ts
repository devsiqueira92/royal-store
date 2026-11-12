import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OriginSelectFieldComponent } from './origin-select-field.component';

describe('OriginSelectFieldComponent', () => {
  let component: OriginSelectFieldComponent;
  let fixture: ComponentFixture<OriginSelectFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OriginSelectFieldComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(OriginSelectFieldComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
