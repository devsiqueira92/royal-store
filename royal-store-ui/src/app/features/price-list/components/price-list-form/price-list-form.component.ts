import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageModule } from 'primeng/message';
import { CheckboxModule } from 'primeng/checkbox';
import { PriceListFacade } from '../../facade/price-list.facade';
import { PaymentMethodFacade } from '@app/features/payment-method/facade/payment-method.facade';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { Options } from '@app/shared/interceptors/options.interface';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { ToggleFieldComponent } from '@app/shared/components/form-fields/toggle-field/toggle-field.component';
import { map, tap } from 'rxjs';

/** −100% makes the item free; anything lower would be negative (API: PriceList.MinDiscountPercentage). */
const MIN_RULE_PERCENTAGE = -100;
/** API: PriceList.MaxDiscountPercentage. */
const MAX_RULE_PERCENTAGE = 999.99;

interface PriceListFormControls {
  id?: FormControl<string | null>;
  name?: FormControl<string | null>;
  basePriceListId?: FormControl<string | null>;
  discountPercentage?: FormControl<number | null>;
  isDefault?: FormControl<boolean | null>;
  isActive?: FormControl<boolean | null>;
  // paymentMethodIds: FormControl<string[] | null>;
}

@Component({
  selector: 'app-price-list-form',
  imports: [
    AsyncPipe,
    ReactiveFormsModule,
    TranslatePipe,
    ButtonModule,
    SelectModule,
    InputNumberModule,
    MessageModule,
    CheckboxModule,
    InputFieldComponent,
    FormLayoutComponent,
    ButtonComponent,
    SelectFieldComponent,
    InputNumberFieldComponent,
    ToggleFieldComponent,
  ],
  templateUrl: './price-list-form.component.html',
  styleUrl: './price-list-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PriceListFormComponent {
  private readonly facade = inject(PriceListFacade);
  private readonly paymentFacade = inject(PaymentMethodFacade);
  private readonly sidebar = inject(DetailsSidebarService);

  protected readonly formGroup = this.buildFormGroup();

  /**
   * The list being edited; null while creating (the list page clears it before opening the
   * drawer). The form is filled once per list, so a later store update never wipes what was typed.
   */
  protected readonly selectedPriceList$ = this.facade.selectedPriceList$.pipe(
    tap(priceList => {
      if (priceList && priceList.id !== this.formGroup.controls.id?.value) {
        this.formGroup.reset({ ...priceList });
      }
    }),
  );

  protected readonly allPriceLists = toSignal(
    this.facade.priceLists$.pipe(
      map(prices => prices.map(p => ({ description: p.name, code: p.id }))),
    ),
    {
      initialValue: [] as Options[],
    },
  );

  protected readonly activePaymentMethods = toSignal(
    this.paymentFacade.activePaymentMethods$,
    {
      initialValue: [],
    },
  );

  constructor() {
    this.paymentFacade.loadPaymentMethods();

    // Closed only once the API accepted the change; on failure the drawer stays open with the input.
    this.facade.saved$.pipe(takeUntilDestroyed()).subscribe(() => this.onClose());
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

    const { id, name, basePriceListId, discountPercentage } =
      this.formGroup.getRawValue();

    if (id) {
      this.facade.updatePriceList(id, {
        name: (name ?? '').trim(),
        basePriceListId: basePriceListId ?? null,
        discountPercentage: discountPercentage ?? null,
      });
    } else {
      this.facade.createPriceList({
        name: (name ?? '').trim(),
        basePriceListId: basePriceListId,
        discountPercentage: discountPercentage,
      });
    }
  }

  private buildFormGroup(): FormGroup<PriceListFormControls> {
    return new FormGroup<PriceListFormControls>({
      id: new FormControl(null),
      name: new FormControl(null, [
        Validators.required,
        Validators.maxLength(100),
      ]),
      basePriceListId: new FormControl(null),
      // Negative = discount, positive = surcharge; same range as the API's PriceList.SetRule.
      discountPercentage: new FormControl(null, [
        Validators.min(MIN_RULE_PERCENTAGE),
        Validators.max(MAX_RULE_PERCENTAGE),
      ]),
      isDefault: new FormControl(null),
      isActive: new FormControl(null),
      // paymentMethodIds: new FormControl<string[]>([]),
    });
  }
}
