import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
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
  selector: 'app-loyalty-tier-form',
  imports: [
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

  protected readonly isCreateMode = computed(
    () =>
      this.detailsSidebarService.contentToRender()?.props?.mode === 'create',
  );

  protected readonly selectedTier = toSignal(
    this.loyaltyTierFacade.selectedLoyaltyTier$,
    { initialValue: null as LoyaltyTier | null },
  );

  // Active is toggled immediately through its own endpoint, never part of the submit.
  protected readonly active = computed(
    () => this.selectedTier()?.isActive ?? false,
  );

  protected readonly formGroup = this.buildFormGroup();

  constructor() {
    effect(() => {
      if (this.isCreateMode()) {
        this.enterCreateMode();
        return;
      }

      this.populateForm(this.selectedTier());
    });
  }

  protected onClose(): void {
    this.loyaltyTierFacade.clearSelectedLoyaltyTier();
    this.detailsSidebarService.closeDrawer();
  }

  protected onActiveChange(active: boolean): void {
    const tier = this.selectedTier();
    if (!tier || active === tier.isActive) {
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

    const { name, cashbackPercentage, cashbackValidityDays } =
      this.formGroup.getRawValue();

    if (this.isCreateMode()) {
      this.loyaltyTierFacade.createLoyaltyTier({
        name: (name ?? '').trim(),
        cashbackPercentage: cashbackPercentage ?? 0,
        cashbackValidityDays: cashbackValidityDays ?? 0,
      });
    } else {
      const tier = this.selectedTier();
      if (!tier) {
        return;
      }

      this.loyaltyTierFacade.updateLoyaltyTier(tier.id, {
        cashbackPercentage: cashbackPercentage ?? 0,
        cashbackValidityDays: cashbackValidityDays ?? 0,
      });
    }

    this.onClose();
  }

  private enterCreateMode(): void {
    this.formGroup.controls.name.enable();
    this.formGroup.reset({
      name: null,
      cashbackPercentage: 0,
      cashbackValidityDays: 0,
    });
  }

  private populateForm(tier: LoyaltyTier | null): void {
    // The API has no endpoint to rename a tier once it exists.
    this.formGroup.controls.name.disable();
    this.formGroup.reset({
      name: tier?.name ?? null,
      cashbackPercentage: tier?.cashbackPercentage ?? 0,
      cashbackValidityDays: tier?.cashbackValidityDays ?? 0,
    });
  }

  private buildFormGroup(): FormGroup<LoyaltyTierFormControls> {
    return new FormGroup<LoyaltyTierFormControls>({
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
