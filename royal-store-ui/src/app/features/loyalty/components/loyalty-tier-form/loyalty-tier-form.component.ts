import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { tap } from 'rxjs';
import { InputNumberModule } from 'primeng/inputnumber';
import { TranslatePipe } from '@ngx-translate/core';
import { LoyaltyTierFacade } from '@app/core/loyalty-tier/facade/loyalty-tier.facade';
import {
  LoyaltyTier,
  LoyaltyTierFormControls,
} from '@app/core/loyalty-tier/interfaces/loyalty-tier.interface';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';

@Component({
  imports: [
    AsyncPipe,
    ReactiveFormsModule,
    InputNumberModule,
    TranslatePipe,
    InputFieldComponent,
    FormLayoutComponent,
    ButtonComponent,
    SwitchComponent,
  ],
  templateUrl: './loyalty-tier-form.component.html',
  styleUrl: './loyalty-tier-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoyaltyTierFormComponent {
  private readonly loyaltyTierFacade = inject(LoyaltyTierFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly formGroup = this.buildFormGroup();

  /**
   * The tier being edited; null while creating (the list clears it before opening the drawer).
   * The form is filled when a tier arrives — only once per tier, so toggling its status (which
   * updates the same tier in the store) never wipes what was typed.
   */
  protected readonly selectedLoyaltyTier$ =
    this.loyaltyTierFacade.selectedLoyaltyTier$.pipe(
      tap(tier => {
        if (tier && tier.id !== this.formGroup.controls.id.value) {
          this.populateForm(tier);
        }
      }),
    );

  constructor() {
    // Closed only once the API accepted the change; on failure the drawer stays open with the input.
    this.loyaltyTierFacade.saved$
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.onClose());
  }

  protected onClose(): void {
    this.loyaltyTierFacade.clearSelectedLoyaltyTier();
    this.detailsSidebarService.closeDrawer();
  }

  // Active is toggled immediately through its own endpoint, never part of the submit.
  protected onActiveChange(tier: LoyaltyTier, active: boolean): void {
    if (active === tier.isActive) {
      return;
    }

    if (active) {
      this.loyaltyTierFacade.activate(tier.id);
    } else {
      this.loyaltyTierFacade.deactivate(tier.id);
    }
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const { id, name, cashbackPercentage, cashbackValidityDays } =
      this.formGroup.getRawValue();

    if (id) {
      this.loyaltyTierFacade.updateLoyaltyTier(id, {
        cashbackPercentage: cashbackPercentage ?? 0,
        cashbackValidityDays: cashbackValidityDays ?? 0,
      });
    } else {
      this.loyaltyTierFacade.createLoyaltyTier({
        name: (name ?? '').trim(),
        cashbackPercentage: cashbackPercentage ?? 0,
        cashbackValidityDays: cashbackValidityDays ?? 0,
      });
    }
  }

  private populateForm(tier: LoyaltyTier): void {
    // The API has no endpoint to rename a tier once it exists.
    this.formGroup.controls.name.disable();
    this.formGroup.reset({
      id: tier.id,
      name: tier.name,
      cashbackPercentage: tier.cashbackPercentage,
      cashbackValidityDays: tier.cashbackValidityDays,
    });
  }

  private buildFormGroup(): FormGroup<LoyaltyTierFormControls> {
    return new FormGroup<LoyaltyTierFormControls>({
      id: new FormControl(null),
      name: new FormControl(null, [
        Validators.required,
        Validators.maxLength(80),
      ]),
      cashbackPercentage: new FormControl(0, [
        Validators.required,
        Validators.min(0),
        Validators.max(100),
      ]),
      cashbackValidityDays: new FormControl(0, [
        Validators.required,
        Validators.min(1),
      ]),
    });
  }
}
