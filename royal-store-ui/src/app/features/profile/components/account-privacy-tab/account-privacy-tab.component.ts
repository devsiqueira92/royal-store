import { ChangeDetectionStrategy, Component, effect, input, output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { PasswordModule } from 'primeng/password';
import { FormErrorComponent } from '@app/shared/components/form-fields/form-error/form-error.component';

interface DeleteAccountFormControls {
  password: FormControl<string | null>;
  acknowledged: FormControl<boolean>;
}

/** Tab 5 — LGPD export and the irreversible account deletion. */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-account-privacy-tab',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    ButtonModule,
    CheckboxModule,
    DialogModule,
    FloatLabelModule,
    PasswordModule,
    FormErrorComponent,
  ],
  templateUrl: './account-privacy-tab.component.html',
  styleUrl: './account-privacy-tab.component.scss',
})
export class AccountPrivacyTabComponent {
  exporting = input<boolean>(false);
  deleting = input<boolean>(false);

  exportData = output<void>();
  deleteAccount = output<string>();

  protected readonly deleteDialogOpen = signal(false);
  protected readonly deleteForm = new FormGroup<DeleteAccountFormControls>({
    password: new FormControl(null, [Validators.required]),
    acknowledged: new FormControl(false, { nonNullable: true, validators: [Validators.requiredTrue] }),
  });

  protected readonly consequences = ['data', 'sales', 'subscription', 'team'];
  private awaitingDelete = false;

  constructor() {
    effect(() => {
      if (!this.deleting() && this.awaitingDelete) {
        this.awaitingDelete = false;
        this.deleteDialogOpen.set(false);
      }
    });
  }

  protected openDeleteDialog(): void {
    this.deleteForm.reset({ password: null, acknowledged: false });
    this.deleteDialogOpen.set(true);
  }

  protected closeDeleteDialog(): void {
    if (!this.deleting()) {
      this.deleteDialogOpen.set(false);
    }
  }

  protected onConfirmDelete(): void {
    if (this.deleteForm.invalid) {
      this.deleteForm.markAllAsTouched();
      return;
    }
    this.awaitingDelete = true;
    this.deleteAccount.emit(this.deleteForm.controls.password.value!);
  }
}
