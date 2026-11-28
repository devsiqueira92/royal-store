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
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputMaskModule } from 'primeng/inputmask';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { DatepickerFieldComponent } from '@app/shared/components/form-fields/datepicker-field/datepicker-field.component';
import { FormErrorComponent } from '@app/shared/components/form-fields/form-error/form-error.component';
import { PlainDatePipe } from '@app/shared/pipes/plain-date.pipe';
import { formatDateOnly, parseDateOnly } from '@app/shared/utils/date-only.util';
import {
  AccountProfileFormControls,
  UpdateProfileRequest,
  UserProfile,
} from '../../interfaces/user-account.interface';
import { formatDocument, initialsOf, maskDocument } from '../../utils/account-format.util';
import { digitsValidator, focusFirstInvalid } from '../../utils/account-validators';

type DocumentMode = 'masked' | 'visible' | 'editing';

const MAX_AVATAR_BYTES = 2 * 1024 * 1024;
const AVATAR_TYPES = ['image/png', 'image/jpeg', 'image/webp'];

/** Tab 1 — identity. Presentational: emits requests, parent talks to the API. */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-account-profile-tab',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    AvatarModule,
    ButtonModule,
    DialogModule,
    FloatLabelModule,
    InputMaskModule,
    TagModule,
    TooltipModule,
    InputFieldComponent,
    DatepickerFieldComponent,
    FormErrorComponent,
    PlainDatePipe,
  ],
  templateUrl: './account-profile-tab.component.html',
  styleUrl: './account-profile-tab.component.scss',
})
export class AccountProfileTabComponent {
  profile = input.required<UserProfile>();
  saving = input<boolean>(false);
  uploadingAvatar = input<boolean>(false);
  removingAvatar = input<boolean>(false);

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  save = output<UpdateProfileRequest>();
  avatarUpload = output<string>();
  avatarRemove = output<void>();

  protected readonly today = new Date();
  protected readonly formGroup = new FormGroup<AccountProfileFormControls>({
    fullName: new FormControl(null, [Validators.required, Validators.maxLength(150)]),
    displayName: new FormControl(null, [Validators.maxLength(60)]),
    document: new FormControl(
      { value: null, disabled: true },
      [Validators.required, digitsValidator([11], 'invalidCpf')],
    ),
    phone: new FormControl(null, [digitsValidator([10, 11], 'invalidPhone')]),
    birthDate: new FormControl<Date | null>(null),
  });

  protected readonly documentMode = signal<DocumentMode>('masked');
  protected readonly initials = computed(() => initialsOf(this.profile().fullName));
  protected readonly documentDisplay = computed(() =>
    this.documentMode() === 'visible'
      ? formatDocument(this.profile().document)
      : maskDocument(this.profile().document),
  );

  protected readonly avatarDialogOpen = signal(false);
  protected readonly avatarPreview = signal<string | null>(null);
  protected readonly avatarError = signal<string | null>(null);
  private awaitingUpload = false;

  constructor() {
    effect(() => this.populate(this.profile()));

    // Upload finished (loading falling edge) → close the dialog.
    effect(() => {
      if (!this.uploadingAvatar() && this.awaitingUpload) {
        this.awaitingUpload = false;
        this.closeAvatarDialog();
      }
    });
  }

  protected toggleDocumentVisibility(): void {
    this.documentMode.update(mode => (mode === 'visible' ? 'masked' : 'visible'));
  }

  protected editDocument(): void {
    this.documentMode.set('editing');
    this.formGroup.controls.document.enable();
  }

  protected onReset(): void {
    this.populate(this.profile());
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      focusFirstInvalid(this.host.nativeElement);
      return;
    }

    const { fullName, displayName, document, phone, birthDate } = this.formGroup.getRawValue();
    this.save.emit({
      fullName: (fullName ?? '').trim(),
      displayName: displayName?.trim() || null,
      document: document ?? this.profile().document,
      phone: phone || null,
      birthDate: formatDateOnly(birthDate),
    });
  }

  protected openAvatarDialog(): void {
    this.avatarPreview.set(null);
    this.avatarError.set(null);
    this.avatarDialogOpen.set(true);
  }

  protected closeAvatarDialog(): void {
    this.avatarDialogOpen.set(false);
  }

  protected onAvatarSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    (event.target as HTMLInputElement).value = '';
    if (!file) {
      return;
    }

    if (!AVATAR_TYPES.includes(file.type)) {
      this.avatarError.set('account.profile.avatar.invalidType');
      return;
    }
    if (file.size > MAX_AVATAR_BYTES) {
      this.avatarError.set('account.profile.avatar.tooLarge');
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      this.avatarError.set(null);
      this.avatarPreview.set(reader.result as string);
    };
    reader.readAsDataURL(file);
  }

  protected confirmAvatar(): void {
    const preview = this.avatarPreview();
    if (!preview) {
      return;
    }
    this.awaitingUpload = true;
    this.avatarUpload.emit(preview);
  }

  private populate(profile: UserProfile): void {
    this.documentMode.set('masked');
    this.formGroup.controls.document.disable();
    this.formGroup.reset({
      fullName: profile.fullName,
      displayName: profile.displayName,
      document: profile.document,
      phone: profile.phone,
      birthDate: parseDateOnly(profile.birthDate),
    });
  }
}
