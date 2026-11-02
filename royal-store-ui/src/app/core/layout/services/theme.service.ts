import { effect, Injectable, signal } from '@angular/core';

const DARK_MODE_CLASS = 'app-dark';
const STORAGE_KEY = 'royal-store-dark-mode';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  readonly isDarkMode = signal<boolean>(this.getInitialDarkMode());

  constructor() {
    effect(() => {
      const isDark = this.isDarkMode();
      document.documentElement.classList.toggle(DARK_MODE_CLASS, isDark);
      localStorage.setItem(STORAGE_KEY, String(isDark));
    });
  }

  toggleDarkMode(): void {
    this.isDarkMode.update(value => !value);
  }

  setDarkMode(isDark: boolean): void {
    this.isDarkMode.set(isDark);
  }

  private getInitialDarkMode(): boolean {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored !== null) {
      return stored === 'true';
    }

    return window.matchMedia('(prefers-color-scheme: dark)').matches;
  }
}
