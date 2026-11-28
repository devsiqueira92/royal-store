import { ChangeDetectionStrategy, Component, effect, input, output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { SelectButtonModule } from 'primeng/selectbutton';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { Options } from '@app/shared/interceptors/options.interface';
import {
  AccountPreferences,
  PreferencesFormControls,
  ThemePreference,
} from '../../interfaces/user-account.interface';
import { TIME_ZONE_OPTIONS } from '../../mocks/user-profile.mock';

interface ThemeOption {
  value: ThemePreference;
  icon: string;
  labelKey: string;
}

interface SwitchRow {
  control: 'emailAlerts' | 'whatsappAlerts' | 'browserAlerts';
  icon: string;
  key: string;
}

interface CheckRow {
  control: 'newsletter' | 'weeklyReports' | 'offers';
  key: string;
}

/** Tab 4 — notification channels, product e-mails, theme and region. */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-account-preferences-tab',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    ButtonModule,
    CheckboxModule,
    SelectButtonModule,
    ToggleSwitchModule,
    SelectFieldComponent,
  ],
  templateUrl: './account-preferences-tab.component.html',
  styleUrl: './account-preferences-tab.component.scss',
})
export class AccountPreferencesTabComponent {
  preferences = input.required<AccountPreferences>();
  saving = input<boolean>(false);

  save = output<AccountPreferences>();

  protected readonly switches: SwitchRow[] = [
    { control: 'emailAlerts', icon: 'pi pi-envelope', key: 'email' },
    { control: 'whatsappAlerts', icon: 'pi pi-whatsapp', key: 'whatsapp' },
    { control: 'browserAlerts', icon: 'pi pi-bell', key: 'browser' },
  ];

  protected readonly checks: CheckRow[] = [
    { control: 'newsletter', key: 'newsletter' },
    { control: 'weeklyReports', key: 'weeklyReports' },
    { control: 'offers', key: 'offers' },
  ];

  protected readonly themeOptions: ThemeOption[] = [
    { value: 'light', icon: 'pi pi-sun', labelKey: 'account.preferences.theme.light' },
    { value: 'dark', icon: 'pi pi-moon', labelKey: 'account.preferences.theme.dark' },
    { value: 'system', icon: 'pi pi-desktop', labelKey: 'account.preferences.theme.system' },
  ];

  protected readonly languageOptions: Options[] = [
    { code: 'pt', description: 'Português (Brasil)' },
    { code: 'en', description: 'English (US)' },
  ];

  protected readonly timeZoneOptions: Options[] = TIME_ZONE_OPTIONS.map(zone => ({
    code: zone,
    description: `${zone.replace('America/', '').replace('_', ' ')} (${this.offsetOf(zone)})`,
  }));

  protected readonly form = new FormGroup<PreferencesFormControls>({
    emailAlerts: new FormControl(true, { nonNullable: true }),
    whatsappAlerts: new FormControl(false, { nonNullable: true }),
    browserAlerts: new FormControl(false, { nonNullable: true }),
    newsletter: new FormControl(false, { nonNullable: true }),
    weeklyReports: new FormControl(false, { nonNullable: true }),
    offers: new FormControl(false, { nonNullable: true }),
    theme: new FormControl<ThemePreference>('system', { nonNullable: true }),
    language: new FormControl<'pt' | 'en'>('pt', { nonNullable: true }),
    timeZone: new FormControl('America/Sao_Paulo', { nonNullable: true }),
  });

  constructor() {
    effect(() => this.populate(this.preferences()));
  }

  protected onReset(): void {
    this.populate(this.preferences());
  }

  protected onSubmit(): void {
    const value = this.form.getRawValue();
    this.save.emit({
      notifications: {
        email: value.emailAlerts,
        whatsapp: value.whatsappAlerts,
        browser: value.browserAlerts,
      },
      communication: {
        newsletter: value.newsletter,
        weeklyReports: value.weeklyReports,
        offers: value.offers,
      },
      theme: value.theme,
      language: value.language,
      timeZone: value.timeZone,
    });
  }

  private populate(preferences: AccountPreferences): void {
    this.form.reset({
      emailAlerts: preferences.notifications.email,
      whatsappAlerts: preferences.notifications.whatsapp,
      browserAlerts: preferences.notifications.browser,
      newsletter: preferences.communication.newsletter,
      weeklyReports: preferences.communication.weeklyReports,
      offers: preferences.communication.offers,
      theme: preferences.theme,
      language: preferences.language,
      timeZone: preferences.timeZone,
    });
  }

  /** `GMT-3` for the zone right now, from the browser's own tz database. */
  private offsetOf(timeZone: string): string {
    const part = new Intl.DateTimeFormat('en-US', { timeZone, timeZoneName: 'shortOffset' })
      .formatToParts(new Date())
      .find(p => p.type === 'timeZoneName');
    return part?.value ?? '';
  }
}
