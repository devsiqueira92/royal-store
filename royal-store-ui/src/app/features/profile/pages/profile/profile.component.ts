import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  OnInit,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { AvatarModule } from 'primeng/avatar';
import { CurrentSellerFacade } from '@app/core/current-seller/facade/current-seller.facade';
import {
  CurrentSeller,
  ProfileFormControls,
} from '@app/core/current-seller/interfaces/current-seller.interface';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-profile',
  imports: [
    DatePipe,
    TranslatePipe,
    AvatarModule,
    ReactiveFormsModule,
    InputFieldComponent,
    FormLayoutComponent,
    ButtonComponent,
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  private readonly currentSellerFacade = inject(CurrentSellerFacade);

  protected readonly currentSeller = toSignal(
    this.currentSellerFacade.currentSeller$,
    { initialValue: null as CurrentSeller | null },
  );
  protected readonly isLoading = toSignal(this.currentSellerFacade.isLoading$, {
    initialValue: false,
  });
  protected readonly isSaving = toSignal(this.currentSellerFacade.isSaving$, {
    initialValue: false,
  });

  protected readonly formGroup = this.buildFormGroup();

  constructor() {
    effect(() => {
      this.populateForm(this.currentSeller());
    });
  }

  ngOnInit(): void {
    this.currentSellerFacade.loadCurrentSeller();
  }

  protected initials(fullName: string): string {
    if (fullName === null) {
      return '';
    }
    return fullName
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part[0])
      .join('')
      .toUpperCase();
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const { fullName, email, phone } = this.formGroup.getRawValue();
    this.currentSellerFacade.updateMyProfile({
      fullName: (fullName ?? '').trim(),
      email: this.normalize(email),
      phone: this.normalize(phone),
    });
  }

  private normalize(value: string | null | undefined): string | null {
    const trimmed = value?.trim();
    return trimmed ? trimmed : null;
  }

  private populateForm(seller: CurrentSeller | null): void {
    this.formGroup.reset({
      fullName: seller?.fullName ?? null,
      email: seller?.email ?? null,
      phone: seller?.phone ?? null,
    });
  }

  private buildFormGroup(): FormGroup<ProfileFormControls> {
    return new FormGroup<ProfileFormControls>({
      fullName: new FormControl(null, [
        Validators.required,
        Validators.maxLength(150),
      ]),
      email: new FormControl(null, [Validators.email]),
      phone: new FormControl(null, [Validators.maxLength(20)]),
    });
  }
}
