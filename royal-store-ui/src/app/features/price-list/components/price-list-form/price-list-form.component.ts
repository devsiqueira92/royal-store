import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  Signal,
  signal,
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageModule } from 'primeng/message';
import { CheckboxModule } from 'primeng/checkbox';
import { PriceList } from '../../interfaces/price-list.interface';
import { PriceListFacade } from '../../facade/price-list.facade';
import { PaymentMethodFacade } from '@app/features/payment-method/facade/payment-method.facade';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { Options } from '@app/shared/interceptors/options.interface';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';

interface PriceListFormControls {
  name: FormControl<string | null>;
  basePriceListId: FormControl<string | null>;
  discountPercentage: FormControl<number | null>;
  paymentMethodIds: FormControl<string[] | null>;
}

@Component({
  selector: 'app-price-list-form',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    SelectModule,
    InputNumberModule,
    MessageModule,
    CheckboxModule,
    InputFieldComponent,
    FormLayoutComponent,
    ButtonComponent,
    SelectFieldComponent,
    InputNumberFieldComponent,
  ],
  templateUrl: './price-list-form.component.html',
  styleUrl: './price-list-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PriceListFormComponent {
  private readonly facade = inject(PriceListFacade);
  private readonly paymentFacade = inject(PaymentMethodFacade);
  private readonly sidebar = inject(DetailsSidebarService);

  protected readonly isCreateMode = computed(
    () => this.sidebar.contentToRender()?.props?.mode === 'create',
  );

  protected readonly selectedPriceList = toSignal(
    this.facade.selectedPriceList$,
    {
      initialValue: null as PriceList | null,
    },
  );

  protected readonly allPriceLists = toSignal(this.facade.priceLists$, {
    initialValue: [] as PriceList[],
  });

  protected readonly activePaymentMethods = toSignal(
    this.paymentFacade.activePaymentMethods$,
    {
      initialValue: [],
    },
  );

  /** Lists that can be used as a base: active, no rule, not `this` list. */
  protected readonly baseOptions: Signal<Options[]> = computed(() => {
    const selected = this.selectedPriceList();
    return this.allPriceLists()
      .filter(p => p.isActive && !p.discountPercentage && p.id !== selected?.id)
      .map(p => ({ description: p.name, code: p.id }));
  });

  protected readonly hasRule = signal(false);

  protected readonly formGroup = this.buildFormGroup();

  constructor() {
    this.paymentFacade.loadPaymentMethods();
    effect(() => {
      const pl = this.selectedPriceList();
      if (this.isCreateMode()) {
        this.formGroup.reset({
          name: null,
          basePriceListId: null,
          discountPercentage: null,
          paymentMethodIds: [],
        });
        this.hasRule.set(false);
      } else if (pl) {
        this.formGroup.reset({
          name: pl.name,
          basePriceListId: pl.basePriceListId,
          discountPercentage: pl.discountPercentage,
          paymentMethodIds: [],
        });
        this.hasRule.set(pl.discountPercentage != null);
      }
    });
  }

  protected toggleRule(): void {
    const next = !this.hasRule();
    this.hasRule.set(next);
    if (!next) {
      this.formGroup.controls.basePriceListId.reset(null);
      this.formGroup.controls.discountPercentage.reset(null);
    }
  }

  protected onClose(): void {
    this.facade.clearSelectedPriceList();
    this.sidebar.closeDrawer();
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const { name, basePriceListId, discountPercentage } =
      this.formGroup.getRawValue();

    if (this.isCreateMode()) {
      this.facade.createPriceList({
        name: (name ?? '').trim(),
        basePriceListId: this.hasRule() ? basePriceListId : null,
        discountPercentage: this.hasRule() ? discountPercentage : null,
      });
      this.sidebar.closeDrawer();
      return;
    }

    const pl = this.selectedPriceList();
    if (!pl) return;

    const trimmedName = (name ?? '').trim();
    if (trimmedName !== pl.name) {
      this.facade.renamePriceList(pl.id, { name: trimmedName });
    }

    if (this.hasRule() && basePriceListId && discountPercentage != null) {
      this.facade.setRule(pl.id, { basePriceListId, discountPercentage });
    } else if (!this.hasRule() && pl.discountPercentage != null) {
      this.facade.clearRule(pl.id);
    }

    this.facade.clearSelectedPriceList();
    this.sidebar.closeDrawer();
  }

  private buildFormGroup(): FormGroup<PriceListFormControls> {
    return new FormGroup<PriceListFormControls>({
      name: new FormControl(null, [
        Validators.required,
        Validators.maxLength(100),
      ]),
      basePriceListId: new FormControl(null),
      discountPercentage: new FormControl(null),
      paymentMethodIds: new FormControl<string[]>([]),
    });
  }
}
