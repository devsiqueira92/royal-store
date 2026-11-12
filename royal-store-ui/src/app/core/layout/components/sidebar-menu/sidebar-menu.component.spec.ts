import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideTranslateService } from '@ngx-translate/core';

import { SidebarMenuComponent } from './sidebar-menu.component';

describe('SidebarMenuComponent', () => {
  let component: SidebarMenuComponent;
  let fixture: ComponentFixture<SidebarMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SidebarMenuComponent],
      // The nav renders translated labels behind router links.
      providers: [provideRouter([]), provideHttpClient(), provideTranslateService({})],
    }).compileComponents();

    fixture = TestBed.createComponent(SidebarMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('only links to routes that exist in the app', () => {
    const routes = fixture.nativeElement.querySelectorAll('a[href]');
    const hrefs = Array.from(routes).map(link => (link as HTMLAnchorElement).getAttribute('href'));

    // `/dashboard` and `/configuration` used to be listed here and had no route.
    expect(hrefs).not.toContain('/dashboard');
    expect(hrefs).not.toContain('/configuration');
  });
});
