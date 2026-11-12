import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DetailsSidebarComponents } from './details-sidebar.component';

describe('DetailsSidebarComponents', () => {
  let component: DetailsSidebarComponents;
  let fixture: ComponentFixture<DetailsSidebarComponents>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetailsSidebarComponents],
    }).compileComponents();

    fixture = TestBed.createComponent(DetailsSidebarComponents);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
