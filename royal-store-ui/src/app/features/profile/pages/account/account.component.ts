import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageModule } from 'primeng/message';
import { SkeletonModule } from 'primeng/skeleton';
import { TabsModule } from 'primeng/tabs';
import { finalize, map, Observable } from 'rxjs';
import { ThemeService } from '@app/core/layout/services/theme.service';
import { NotificationService } from '@app/shared/services/notification.service';
import {
  AccountAction,
  AccountPreferences,
  AccountTab,
  BillingDetails,
  ChangePasswordRequest,
  ThemePreference,
  TwoFactorSetup,
  UpdateProfileRequest,
  UserAccount,
} from '../../interfaces/user-account.interface';
import {
  MOCK_CURRENT_PASSWORD,
  UserAccountMockService,
} from '../../services/user-account-mock.service';
import { AccountProfileTabComponent } from '../../components/account-profile-tab/account-profile-tab.component';
import { AccountSecurityTabComponent } from '../../components/account-security-tab/account-security-tab.component';
import {
  AccountBillingTabComponent,
  InvoiceDownload,
} from '../../components/account-billing-tab/account-billing-tab.component';
import { AccountPreferencesTabComponent } from '../../components/account-preferences-tab/account-preferences-tab.component';
import { AccountPrivacyTabComponent } from '../../components/account-privacy-tab/account-privacy-tab.component';

const TABS: readonly AccountTab[] = ['profile', 'security', 'billing', 'preferences', 'privacy'];

/**
 * Account management (`/profile/account`) — layout-validation build.
 *
 * Smart component: owns the account state (signals), the pending-action set
 * and every call to `UserAccountMockService`. The five tabs are presentational
 * and only emit intents. The active tab mirrors `?tab=` so a review link can
 * open straight on, say, billing.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-account',
  imports: [
    TranslatePipe,
    ConfirmDialogModule,
    MessageModule,
    SkeletonModule,
    TabsModule,
    AccountProfileTabComponent,
    AccountSecurityTabComponent,
    AccountBillingTabComponent,
    AccountPreferencesTabComponent,
    AccountPrivacyTabComponent,
  ],
  providers: [ConfirmationService],
  templateUrl: './account.component.html',
  styleUrl: './account.component.scss',
})
export class AccountComponent implements OnInit {
  private readonly accountService = inject(UserAccountMockService);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly themeService = inject(ThemeService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly tabs = TABS;
  protected readonly mockPassword = MOCK_CURRENT_PASSWORD;

  protected readonly account = signal<UserAccount | null>(null);
  protected readonly twoFactorSetup = signal<TwoFactorSetup | null>(null);
  private readonly pending = signal<ReadonlySet<AccountAction>>(new Set());

  protected readonly activeTab = toSignal(
    this.route.queryParamMap.pipe(
      map(params => {
        const tab = params.get('tab') as AccountTab | null;
        return tab && TABS.includes(tab) ? tab : 'profile';
      }),
    ),
    { initialValue: 'profile' as AccountTab },
  );

  ngOnInit(): void {
    this.accountService
      .getAccount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(account => this.account.set(account));
  }

  protected isPending(action: AccountAction): boolean {
    return this.pending().has(action);
  }

  protected onTabChange(tab: string | number | undefined): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: tab === 'profile' ? null : tab },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  /* ---------- Profile ---------- */

  protected onSaveProfile(request: UpdateProfileRequest): void {
    this.run('saveProfile', this.accountService.updateProfile(request), profile => {
      this.patch({ profile });
      this.toastSuccess('profileSaved');
    });
  }

  protected onUploadAvatar(dataUrl: string): void {
    this.run('uploadAvatar', this.accountService.uploadAvatar(dataUrl), profile => {
      this.patch({ profile });
      this.toastSuccess('avatarUploaded');
    });
  }

  protected onRemoveAvatar(): void {
    this.run('removeAvatar', this.accountService.removeAvatar(), profile => {
      this.patch({ profile });
      this.toastSuccess('avatarRemoved');
    });
  }

  /* ---------- Security ---------- */

  protected onChangePassword(request: ChangePasswordRequest): void {
    this.run('changePassword', this.accountService.changePassword(request), security => {
      this.patch({ security });
      this.toastSuccess('passwordChanged');
    });
  }

  protected onStartTwoFactor(): void {
    this.twoFactorSetup.set(null);
    this.run('startTwoFactor', this.accountService.startTwoFactorSetup(), setup =>
      this.twoFactorSetup.set(setup),
    );
  }

  protected onConfirmTwoFactor(code: string): void {
    this.run('enableTwoFactor', this.accountService.confirmTwoFactor(code), security => {
      this.patch({ security });
      this.twoFactorSetup.set(null);
      this.toastSuccess('twoFactorEnabled');
    });
  }

  protected onDisableTwoFactor(): void {
    this.confirm('disableTwoFactor', () =>
      this.run('disableTwoFactor', this.accountService.disableTwoFactor(), security => {
        this.patch({ security });
        this.toastSuccess('twoFactorDisabled', 'warn');
      }),
    );
  }

  protected onEndOtherSessions(): void {
    this.confirm('endOtherSessions', () =>
      this.run('endOtherSessions', this.accountService.endOtherSessions(), security => {
        this.patch({ security });
        this.toastSuccess('sessionsEnded');
      }),
    );
  }

  /* ---------- Billing ---------- */

  protected onSaveBilling(details: BillingDetails): void {
    this.run('saveBilling', this.accountService.updateBillingDetails(details), billing => {
      this.patch({ billing });
      this.toastSuccess('billingSaved');
    });
  }

  protected onUpgrade(): void {
    this.toastSuccess('upgradeSimulated');
  }

  protected onCancelPlan(): void {
    this.confirm('cancelPlan', () => this.toastSuccess('cancelSimulated', 'warn'));
  }

  protected onChangeCard(): void {
    this.toastSuccess('cardSimulated');
  }

  protected onDownload({ invoice, document }: InvoiceDownload): void {
    this.notification.success({
      summary: this.translate.instant('account.toasts.download.summary'),
      detail: this.translate.instant('account.toasts.download.detail', {
        document: this.translate.instant(`account.billing.invoices.download.${document}`),
        number: invoice.number,
      }),
    });
  }

  /* ---------- Preferences ---------- */

  protected onSavePreferences(preferences: AccountPreferences): void {
    this.run('savePreferences', this.accountService.updatePreferences(preferences), saved => {
      this.patch({ preferences: saved });
      this.applyTheme(saved.theme);
      // Switch language first so the toast already reads in the new one.
      this.translate.use(saved.language).subscribe(() => this.toastSuccess('preferencesSaved'));
    });
  }

  /* ---------- Privacy ---------- */

  protected onExportData(): void {
    this.run('exportData', this.accountService.requestDataExport(), () =>
      this.toastSuccess('exportRequested'),
    );
  }

  protected onDeleteAccount(password: string): void {
    this.run('deleteAccount', this.accountService.deleteAccount(password), () =>
      this.toastSuccess('accountDeleted', 'warn'),
    );
  }

  /* ---------- Helpers ---------- */

  private run<T>(action: AccountAction, source: Observable<T>, onSuccess: (value: T) => void): void {
    this.pending.update(set => new Set(set).add(action));
    source
      .pipe(
        finalize(() =>
          this.pending.update(set => {
            const next = new Set(set);
            next.delete(action);
            return next;
          }),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: onSuccess,
        error: (error: Error) =>
          this.notification.error({
            summary: this.translate.instant('account.toasts.error.summary'),
            detail: this.translate.instant(`account.toasts.error.${error.message}`),
          }),
      });
  }

  private patch(slice: Partial<UserAccount>): void {
    this.account.update(account => (account ? { ...account, ...slice } : account));
  }

  private confirm(key: 'disableTwoFactor' | 'endOtherSessions' | 'cancelPlan', accept: () => void): void {
    this.confirmation.confirm({
      key: 'account',
      header: this.translate.instant(`account.confirm.${key}.header`),
      message: this.translate.instant(`account.confirm.${key}.message`),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.translate.instant(`account.confirm.${key}.accept`),
      rejectLabel: this.translate.instant('account.actions.cancel'),
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', text: true },
      accept,
    });
  }

  private toastSuccess(key: string, severity: 'success' | 'warn' = 'success'): void {
    this.notification[severity]({
      summary: this.translate.instant(`account.toasts.${key}.summary`),
      detail: this.translate.instant(`account.toasts.${key}.detail`),
    });
  }

  private applyTheme(theme: ThemePreference): void {
    const dark =
      theme === 'system' ? window.matchMedia('(prefers-color-scheme: dark)').matches : theme === 'dark';
    this.themeService.setDarkMode(dark);
  }
}
