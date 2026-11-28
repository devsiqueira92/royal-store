import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  ElementRef,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { PasswordModule } from 'primeng/password';
import { TagModule } from 'primeng/tag';
import { TimelineModule } from 'primeng/timeline';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormErrorComponent } from '@app/shared/components/form-fields/form-error/form-error.component';
import { PlainDatePipe } from '@app/shared/pipes/plain-date.pipe';
import { passwordsMatchValidator } from '@app/shared/validators/password-match.validator';
import {
  AccountSecurity,
  ChangePasswordFormControls,
  ChangePasswordRequest,
  DeviceType,
  SecurityEventType,
  TwoFactorSetup,
} from '../../interfaces/user-account.interface';
import { TwoFactorSetupDialogComponent } from '../two-factor-setup-dialog/two-factor-setup-dialog.component';
import {
  focusFirstInvalid,
  passwordRulesOf,
  passwordRulesValidator,
} from '../../utils/account-validators';

const DEVICE_ICONS: Record<DeviceType, string> = {
  desktop: 'pi pi-desktop',
  mobile: 'pi pi-mobile',
  tablet: 'pi pi-tablet',
};

const EVENT_ICONS: Record<SecurityEventType, string> = {
  login: 'pi pi-sign-in',
  failedLogin: 'pi pi-times-circle',
  passwordChanged: 'pi pi-key',
  twoFactorEnabled: 'pi pi-shield',
  twoFactorDisabled: 'pi pi-shield',
  newDevice: 'pi pi-desktop',
  sessionsEnded: 'pi pi-sign-out',
};

const RISKY_EVENTS: SecurityEventType[] = ['failedLogin', 'twoFactorDisabled', 'newDevice'];

/** Tab 2 — password, 2FA, sessions and the security log. */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-account-security-tab',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    TranslatePipe,
    ButtonModule,
    DialogModule,
    FloatLabelModule,
    PasswordModule,
    TagModule,
    TimelineModule,
    FormErrorComponent,
    PlainDatePipe,
    TwoFactorSetupDialogComponent,
  ],
  templateUrl: './account-security-tab.component.html',
  styleUrl: './account-security-tab.component.scss',
})
export class AccountSecurityTabComponent {
  security = input.required<AccountSecurity>();
  twoFactorSetup = input<TwoFactorSetup | null>(null);
  changingPassword = input<boolean>(false);
  startingTwoFactor = input<boolean>(false);
  confirmingTwoFactor = input<boolean>(false);
  disablingTwoFactor = input<boolean>(false);
  endingSessions = input<boolean>(false);

  changePassword = output<ChangePasswordRequest>();
  startTwoFactor = output<void>();
  confirmTwoFactor = output<string>();
  disableTwoFactor = output<void>();
  endOtherSessions = output<void>();

  protected readonly deviceIcons = DEVICE_ICONS;

  protected readonly passwordForm = new FormGroup<ChangePasswordFormControls>(
    {
      currentPassword: new FormControl(null, [Validators.required]),
      password: new FormControl(null, [Validators.required, passwordRulesValidator]),
      confirmPassword: new FormControl(null, [Validators.required]),
    },
    { validators: passwordsMatchValidator() },
  );

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Live checklist under the strength meter: which rule is still missing. */
  private readonly newPassword = toSignal(this.passwordForm.controls.password.valueChanges, {
    initialValue: null,
  });
  protected readonly ruleChecks = computed(() => {
    const rules = passwordRulesOf(this.newPassword());
    return [
      { key: 'length', met: rules.length },
      { key: 'letter', met: rules.letter },
      { key: 'number', met: rules.number },
    ];
  });

  protected readonly twoFactorEnabled = computed(() => this.security().twoFactor.enabled);
  protected readonly otherSessions = computed(
    () => this.security().sessions.filter(session => !session.isCurrent).length,
  );

  protected readonly setupOpen = signal(false);
  protected readonly backupCodesOpen = signal(false);
  protected readonly codesCopied = signal(false);

  private readonly passwordChangedAt = computed(() => this.security().passwordChangedAt);

  constructor() {
    // A new `passwordChangedAt` means the change went through: clear the form.
    effect(() => {
      this.passwordChangedAt();
      this.passwordForm.reset();
    });

    // Enrolment confirmed → swap the setup dialog for the backup codes.
    effect(() => {
      if (this.twoFactorEnabled() && this.setupOpen()) {
        this.setupOpen.set(false);
        this.backupCodesOpen.set(true);
      }
    });
  }

  protected eventIcon(type: SecurityEventType): string {
    return EVENT_ICONS[type];
  }

  protected isRisky(type: SecurityEventType): boolean {
    return RISKY_EVENTS.includes(type);
  }

  protected onChangePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      focusFirstInvalid(this.host.nativeElement);
      return;
    }
    const { currentPassword, password } = this.passwordForm.getRawValue();
    this.changePassword.emit({ currentPassword: currentPassword!, password: password! });
  }

  protected onStartTwoFactor(): void {
    this.setupOpen.set(true);
    this.startTwoFactor.emit();
  }

  protected openBackupCodes(): void {
    this.codesCopied.set(false);
    this.backupCodesOpen.set(true);
  }

  protected async copyBackupCodes(): Promise<void> {
    await navigator.clipboard.writeText(this.security().twoFactor.backupCodes.join('\n'));
    this.codesCopied.set(true);
  }

  protected downloadBackupCodes(): void {
    const content = `Royal Store — backup codes\n\n${this.security().twoFactor.backupCodes.join('\n')}\n`;
    const url = URL.createObjectURL(new Blob([content], { type: 'text/plain' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = 'royal-store-backup-codes.txt';
    link.click();
    URL.revokeObjectURL(url);
  }
}
