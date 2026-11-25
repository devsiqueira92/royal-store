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
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
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
import { ToggleFieldComponent } from '@app/shared/components/form-fields/toggle-field/toggle-field.component';
import { map } from 'rxjs';

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

  protected readonly isCreateMode = computed(
    () => this.sidebar.contentToRender()?.props?.mode === 'create',
  );

  protected readonly selectedPriceList = toSignal(
    this.facade.selectedPriceList$,
    {
      initialValue: null as PriceList | null,
    },
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

  protected readonly isDefault: Signal<boolean | undefined> = computed(() => {
    const selected = this.selectedPriceList();
    return selected?.isDefault;
  });

  protected readonly formGroup = this.buildFormGroup();

  constructor() {
    this.paymentFacade.loadPaymentMethods();
    this.formGroup.patchValue({ ...this.selectedPriceList() });
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

    const { id, name, basePriceListId, discountPercentage, isDefault } =
      this.formGroup.getRawValue();

    if (this.isCreateMode()) {
      this.facade.createPriceList({
        name: (name ?? '').trim(),
        basePriceListId: basePriceListId,
        discountPercentage: discountPercentage,
      });
    } else {
      this.facade.updatePriceList(id!, {
        name: (name ?? '').trim(),
        basePriceListId: basePriceListId ?? null,
        discountPercentage: discountPercentage ?? null,
      });

      // if (isDefault && !this.isDefault()) {
      //   this.facade.setDefault(id!);
      // }
    }

    this.facade.clearSelectedPriceList();
    this.sidebar.closeDrawer();
  }

  private buildFormGroup(): FormGroup<PriceListFormControls> {
    return new FormGroup<PriceListFormControls>({
      id: new FormControl(null),
      name: new FormControl(null, [
        Validators.required,
        Validators.maxLength(100),
      ]),
      basePriceListId: new FormControl(null),
      discountPercentage: new FormControl(null),
      isDefault: new FormControl(null),
      isActive: new FormControl(null),
      // paymentMethodIds: new FormControl<string[]>([]),
    });
  }
}
