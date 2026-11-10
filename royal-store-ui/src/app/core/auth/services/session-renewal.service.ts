import { effect, inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthFacade } from '../facade/auth.facade';
import { AuthService } from './auth.service';

const ACTIVITY_EVENTS = [
  'mousedown',
  'keydown',
  'wheel',
  'touchstart',
  'scroll',
] as const;

// Well under the backend's 60-minute token TTL (Jwt:ExpiryMinutes), so a
// check always lands on a token the `/auth/refresh` endpoint still accepts
// (it requires an already-valid session).
const CHECK_INTERVAL_MS = 5 * 60 * 1000;

// No tracked activity for this long means the tab was left open and
// forgotten: stop renewing and let the session log the user out instead of
// keeping it alive forever.
const INACTIVITY_THRESHOLD_MS = 20 * 60 * 1000;

/**
 * Keeps the session cookie alive with periodic silent `/auth/refresh` calls
 * while the user is active, so they're never bounced to the login page just
 * because the access token's TTL elapsed mid-session. Idle too long and the
 * renewal stops, so the session expires and the next request's 401 sends
 * them to login as usual.
 *
 * Instantiated once from the app root so it runs for the whole session —
 * see `App`'s constructor.
 */
@Injectable({
  providedIn: 'root',
})
export class SessionRenewalService {
  private readonly authService = inject(AuthService);
  private readonly authFacade = inject(AuthFacade);

  private readonly isAuthenticated = toSignal(
    this.authFacade.isAuthenticated$,
    { initialValue: false },
  );

  private lastActivityAt = Date.now();
  private intervalId: ReturnType<typeof setInterval> | null = null;

  constructor() {
    for (const eventName of ACTIVITY_EVENTS) {
      document.addEventListener(eventName, this.registerActivity, {
        passive: true,
      });
    }

    effect(() => {
      if (this.isAuthenticated()) {
        this.registerActivity();
        this.scheduleChecks();
      } else {
        this.stopChecks();
      }
    });
  }

  private registerActivity = (): void => {
    this.lastActivityAt = Date.now();
  };

  private scheduleChecks(): void {
    if (this.intervalId) {
      return;
    }

    this.intervalId = setInterval(() => this.checkSession(), CHECK_INTERVAL_MS);
  }

  private stopChecks(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }

  private checkSession(): void {
    const idleFor = Date.now() - this.lastActivityAt;

    if (idleFor >= INACTIVITY_THRESHOLD_MS) {
      this.stopChecks();
      this.authFacade.logout();
      return;
    }

    this.authService.refreshSession().subscribe({
      // A failed refresh means the session is already dead; the error
      // interceptor sends the user to login on the next request either way.
      error: () => this.stopChecks(),
    });
  }
}
