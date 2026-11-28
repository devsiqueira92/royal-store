import { ChangeDetectionStrategy, Component, computed, effect, input, output } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputOtpModule } from 'primeng/inputotp';
import { SkeletonModule } from 'primeng/skeleton';
import qrcode from 'qrcode-generator';
import { TwoFactorSetup } from '../../interfaces/user-account.interface';

/**
 * TOTP enrolment: scan the QR (or type the key), then prove it with a code.
 * The QR is rendered locally from the `otpauth://` URL — the secret never
 * leaves the browser for a third-party QR service.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-two-factor-setup-dialog',
  imports: [ReactiveFormsModule, TranslatePipe, ButtonModule, DialogModule, InputOtpModule, SkeletonModule],
  templateUrl: './two-factor-setup-dialog.component.html',
  styleUrl: './two-factor-setup-dialog.component.scss',
})
export class TwoFactorSetupDialogComponent {
  visible = input<boolean>(false);
  setup = input<TwoFactorSetup | null>(null);
  confirming = input<boolean>(false);

  confirmed = output<string>();
  closed = output<void>();

  protected readonly code = new FormControl<string | null>(null, [
    Validators.required,
    Validators.pattern(/^\d{6}$/),
  ]);

  protected readonly qrDataUrl = computed(() => {
    const url = this.setup()?.otpauthUrl;
    if (!url) {
      return null;
    }
    const qr = qrcode(0, 'M');
    qr.addData(url);
    qr.make();
    return qr.createDataURL(5, 2);
  });

  /** `JBSW Y3DP …` — groups of four are far easier to type by hand. */
  protected readonly groupedSecret = computed(
    () => this.setup()?.secret.match(/.{1,4}/g)?.join(' ') ?? '',
  );

  constructor() {
    effect(() => {
      if (this.visible()) {
        this.code.reset();
      }
    });
  }

  protected onConfirm(): void {
    if (this.code.invalid || this.confirming()) {
      this.code.markAsTouched();
      return;
    }
    this.confirmed.emit(this.code.value!);
  }

  protected onClose(): void {
    if (!this.confirming()) {
      this.closed.emit();
    }
  }
}
