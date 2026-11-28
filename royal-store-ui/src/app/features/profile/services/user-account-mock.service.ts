import { Injectable } from '@angular/core';
import { delay, Observable, of, throwError } from 'rxjs';
import { formatDateOnly } from '@app/shared/utils/date-only.util';
import {
  AccountBilling,
  AccountPreferences,
  AccountSecurity,
  BillingDetails,
  ChangePasswordRequest,
  SecurityEvent,
  SecurityEventType,
  TwoFactorSetup,
  UpdateProfileRequest,
  UserAccount,
  UserProfile,
} from '../interfaces/user-account.interface';
import { USER_ACCOUNT_MOCK } from '../mocks/user-profile.mock';

/** Simulated round-trip, long enough to see every loading state. */
const LATENCY_MS = 700;

/** Accepted by the mock as "the current password"; anything else fails. */
export const MOCK_CURRENT_PASSWORD = 'royal123';

/**
 * Stand-in for the future `api/me/*` endpoints. Same shape a real HTTP service
 * would have (cold Observables), so swapping it later touches no component.
 * State lives in a private copy for the session, so edits survive tab changes.
 */
@Injectable({ providedIn: 'root' })
export class UserAccountMockService {
  private state: UserAccount = structuredClone(USER_ACCOUNT_MOCK);

  getAccount(): Observable<UserAccount> {
    return this.respond(this.state);
  }

  updateProfile(request: UpdateProfileRequest): Observable<UserProfile> {
    this.state.profile = { ...this.state.profile, ...request };
    return this.respond(this.state.profile);
  }

  uploadAvatar(dataUrl: string): Observable<UserProfile> {
    this.state.profile = { ...this.state.profile, avatarUrl: dataUrl };
    return this.respond(this.state.profile, LATENCY_MS * 2);
  }

  removeAvatar(): Observable<UserProfile> {
    this.state.profile = { ...this.state.profile, avatarUrl: null };
    return this.respond(this.state.profile);
  }

  changePassword(request: ChangePasswordRequest): Observable<AccountSecurity> {
    if (request.currentPassword !== MOCK_CURRENT_PASSWORD) {
      return throwError(() => new Error('invalidCurrentPassword')).pipe(delay(LATENCY_MS));
    }

    this.state.security = {
      ...this.state.security,
      passwordChangedAt: formatDateOnly(new Date())!,
      activity: this.withEvent('passwordChanged'),
    };
    return this.respond(this.state.security);
  }

  startTwoFactorSetup(): Observable<TwoFactorSetup> {
    const secret = this.randomBase32(32);
    const label = encodeURIComponent(`Royal Store:${this.state.profile.email}`);
    return this.respond({
      secret,
      otpauthUrl: `otpauth://totp/${label}?secret=${secret}&issuer=Royal%20Store&digits=6&period=30`,
    });
  }

  /** Any 6 digits pass: there is no real TOTP verification in the mock. */
  confirmTwoFactor(code: string): Observable<AccountSecurity> {
    if (!/^\d{6}$/.test(code)) {
      return throwError(() => new Error('invalidCode')).pipe(delay(LATENCY_MS));
    }

    this.state.security = {
      ...this.state.security,
      twoFactor: {
        enabled: true,
        enabledAt: formatDateOnly(new Date()),
        backupCodes: Array.from({ length: 8 }, () =>
          `${this.randomBase32(4)}-${this.randomBase32(4)}`.toLowerCase(),
        ),
      },
      activity: this.withEvent('twoFactorEnabled'),
    };
    return this.respond(this.state.security);
  }

  disableTwoFactor(): Observable<AccountSecurity> {
    this.state.security = {
      ...this.state.security,
      twoFactor: { enabled: false, enabledAt: null, backupCodes: [] },
      activity: this.withEvent('twoFactorDisabled'),
    };
    return this.respond(this.state.security);
  }

  endOtherSessions(): Observable<AccountSecurity> {
    this.state.security = {
      ...this.state.security,
      sessions: this.state.security.sessions.filter(session => session.isCurrent),
      activity: this.withEvent('sessionsEnded'),
    };
    return this.respond(this.state.security);
  }

  updateBillingDetails(details: BillingDetails): Observable<AccountBilling> {
    this.state.billing = { ...this.state.billing, details };
    return this.respond(this.state.billing);
  }

  updatePreferences(preferences: AccountPreferences): Observable<AccountPreferences> {
    this.state.preferences = preferences;
    return this.respond(this.state.preferences);
  }

  requestDataExport(): Observable<void> {
    return this.respond(undefined, LATENCY_MS * 2);
  }

  deleteAccount(password: string): Observable<void> {
    return password === MOCK_CURRENT_PASSWORD
      ? this.respond(undefined, LATENCY_MS * 2)
      : throwError(() => new Error('invalidCurrentPassword')).pipe(delay(LATENCY_MS));
  }

  /** Deep copy so callers never hold a reference into the mock state. */
  private respond<T>(value: T, latency = LATENCY_MS): Observable<T> {
    return of(structuredClone(value)).pipe(delay(latency));
  }

  private withEvent(type: SecurityEventType): SecurityEvent[] {
    const current = this.state.security.sessions.find(session => session.isCurrent);
    const event: SecurityEvent = {
      id: `evt_${Date.now()}`,
      type,
      occurredAt: new Date().toISOString(),
      ip: current?.ip ?? '—',
      location: current?.location ?? '—',
      device: current ? `${current.browser} · ${current.os}` : '—',
    };
    return [event, ...this.state.security.activity];
  }

  private randomBase32(length: number): string {
    const alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ234567';
    const bytes = crypto.getRandomValues(new Uint8Array(length));
    return Array.from(bytes, byte => alphabet[byte % 32]).join('');
  }
}
