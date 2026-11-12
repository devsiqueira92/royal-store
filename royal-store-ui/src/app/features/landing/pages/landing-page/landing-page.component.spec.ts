import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { provideHttpClient } from '@angular/common/http';
import { describe, expect, it } from 'vitest';
import { LandingPageComponent } from './landing-page.component';

describe('LandingPageComponent', () => {
  it('renders every landing section without template errors', async () => {
    await TestBed.configureTestingModule({
      imports: [LandingPageComponent],
      providers: [provideRouter([]), provideHttpClient(), provideTranslateService({})],
    }).compileComponents();

    const fixture = TestBed.createComponent(LandingPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const host = fixture.nativeElement as HTMLElement;

    expect(host.querySelector('h1')).toBeTruthy();
    expect(host.querySelectorAll('app-dashboard-mockup').length).toBeGreaterThan(0);
    expect(host.querySelectorAll('.plan').length).toBe(3);
    expect(host.querySelectorAll('p-accordion-panel').length).toBe(7);
    expect(host.querySelector('.landing__skip')).toBeTruthy();
  });
});
