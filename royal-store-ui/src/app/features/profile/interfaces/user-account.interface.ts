import { FormControl } from '@angular/forms';

/**
 * Read/write models of the account page (`/profile/account`).
 *
 * Layout-validation stage: everything is served by `UserAccountMockService`.
 * Date-only values (`birthDate`, `renewalDate`, invoice dates) are plain
 * `yyyy-MM-dd` strings, as the API's `DateOnly` sends them. Timestamps
 * (`lastSeenAt`, `occurredAt`) are full ISO strings with offset.
 */

export type AccountTab = 'profile' | 'security' | 'billing' | 'preferences' | 'privacy';

export type AccountAction =
  | 'saveProfile'
  | 'uploadAvatar'
  | 'removeAvatar'
  | 'changePassword'
  | 'startTwoFactor'
  | 'enableTwoFactor'
  | 'disableTwoFactor'
  | 'endOtherSessions'
  | 'saveBilling'
  | 'savePreferences'
  | 'exportData'
  | 'deleteAccount';

export type CompanyRole = 'owner' | 'admin' | 'manager' | 'seller';

export interface UserProfile {
  id: string;
  fullName: string;
  displayName: string | null;
  email: string;
  /** Digits only. 11 = CPF, 14 = CNPJ. */
  document: string;
  /** Digits only, DDD included. */
  phone: string | null;
  /** `yyyy-MM-dd` (`DateOnly`). */
  birthDate: string | null;
  role: CompanyRole;
  avatarUrl: string | null;
  /** `yyyy-MM-dd`. */
  memberSince: string;
}

export type DeviceType = 'desktop' | 'mobile' | 'tablet';

export interface ActiveSession {
  id: string;
  deviceType: DeviceType;
  browser: string;
  os: string;
  ip: string;
  location: string;
  /** ISO timestamp. */
  lastSeenAt: string;
  isCurrent: boolean;
}

export type SecurityEventType =
  | 'login'
  | 'failedLogin'
  | 'passwordChanged'
  | 'twoFactorEnabled'
  | 'twoFactorDisabled'
  | 'newDevice'
  | 'sessionsEnded';

export interface SecurityEvent {
  id: string;
  type: SecurityEventType;
  /** ISO timestamp. */
  occurredAt: string;
  ip: string;
  location: string;
  device: string;
}

export interface TwoFactorState {
  enabled: boolean;
  /** `yyyy-MM-dd`, null while disabled. */
  enabledAt: string | null;
  backupCodes: string[];
}

export interface AccountSecurity {
  /** `yyyy-MM-dd`. */
  passwordChangedAt: string;
  twoFactor: TwoFactorState;
  sessions: ActiveSession[];
  activity: SecurityEvent[];
}

/** Returned by the setup step: what the authenticator app needs. */
export interface TwoFactorSetup {
  secret: string;
  otpauthUrl: string;
}

export type PlanStatus = 'active' | 'trialing' | 'pastDue' | 'cancelled';
export type BillingCycle = 'monthly' | 'yearly';

export interface SubscriptionPlan {
  name: string;
  status: PlanStatus;
  cycle: BillingCycle;
  price: number;
  /** `yyyy-MM-dd`. */
  renewalDate: string;
}

export type UsageMetricKey = 'invoicesIssued' | 'users' | 'storage';

export interface UsageMetric {
  key: UsageMetricKey;
  used: number;
  limit: number;
  /** Display unit; null for plain counts. */
  unit: 'GB' | null;
}

export interface BillingAddress {
  zipCode: string;
  street: string;
  number: string;
  complement: string | null;
  district: string;
  city: string;
  state: string;
}

export interface BillingDetails {
  legalName: string;
  /** Digits only. */
  cnpj: string;
  /** Digits, or `ISENTO`. */
  stateRegistration: string;
  financeEmail: string;
  address: BillingAddress;
}

export interface PaymentCard {
  brand: string;
  last4: string;
  /** `MM/yy`. */
  expiry: string;
  holderName: string;
}

export type InvoiceStatus = 'paid' | 'pending' | 'cancelled';
export type InvoiceDocument = 'pdf' | 'boleto' | 'xml';

export interface Invoice {
  id: string;
  number: string;
  /** `yyyy-MM-dd`. */
  issuedAt: string;
  /** `yyyy-MM-dd`. */
  dueDate: string;
  amount: number;
  status: InvoiceStatus;
  description: string;
}

export interface AccountBilling {
  plan: SubscriptionPlan;
  usage: UsageMetric[];
  details: BillingDetails;
  card: PaymentCard;
  invoices: Invoice[];
}

export type ThemePreference = 'light' | 'dark' | 'system';
export type LanguagePreference = 'pt' | 'en';

export interface AccountPreferences {
  notifications: {
    email: boolean;
    whatsapp: boolean;
    browser: boolean;
  };
  communication: {
    newsletter: boolean;
    weeklyReports: boolean;
    offers: boolean;
  };
  theme: ThemePreference;
  language: LanguagePreference;
  timeZone: string;
}

export interface UserAccount {
  profile: UserProfile;
  security: AccountSecurity;
  billing: AccountBilling;
  preferences: AccountPreferences;
}

export interface UpdateProfileRequest {
  fullName: string;
  displayName: string | null;
  document: string;
  phone: string | null;
  birthDate: string | null;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  password: string;
}

/* ---------- Form controls ---------- */

export interface AccountProfileFormControls {
  fullName: FormControl<string | null>;
  displayName: FormControl<string | null>;
  document: FormControl<string | null>;
  phone: FormControl<string | null>;
  /** Datepicker binds `Date`; converted to `yyyy-MM-dd` on submit. */
  birthDate: FormControl<Date | null>;
}

export interface ChangePasswordFormControls {
  currentPassword: FormControl<string | null>;
  password: FormControl<string | null>;
  confirmPassword: FormControl<string | null>;
}

export interface BillingDetailsFormControls {
  legalName: FormControl<string | null>;
  cnpj: FormControl<string | null>;
  stateRegistration: FormControl<string | null>;
  financeEmail: FormControl<string | null>;
  zipCode: FormControl<string | null>;
  street: FormControl<string | null>;
  number: FormControl<string | null>;
  complement: FormControl<string | null>;
  district: FormControl<string | null>;
  city: FormControl<string | null>;
  state: FormControl<string | null>;
}

export interface PreferencesFormControls {
  emailAlerts: FormControl<boolean>;
  whatsappAlerts: FormControl<boolean>;
  browserAlerts: FormControl<boolean>;
  newsletter: FormControl<boolean>;
  weeklyReports: FormControl<boolean>;
  offers: FormControl<boolean>;
  theme: FormControl<ThemePreference>;
  language: FormControl<LanguagePreference>;
  timeZone: FormControl<string>;
}
