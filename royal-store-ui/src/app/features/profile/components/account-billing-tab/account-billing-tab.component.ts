import {
  ChangeDetectionStrategy,
  Component,
  effect,
  ElementRef,
  inject,
  input,
  output,
} from '@angular/core';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputMaskModule } from 'primeng/inputmask';
import { ProgressBarModule } from 'primeng/progressbar';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { FormErrorComponent } from '@app/shared/components/form-fields/form-error/form-error.component';
import { Options } from '@app/shared/interceptors/options.interface';
import { PlainDatePipe } from '@app/shared/pipes/plain-date.pipe';
import {
  AccountBilling,
  BillingDetails,
  BillingDetailsFormControls,
  Invoice,
  InvoiceDocument,
  InvoiceStatus,
  PlanStatus,
  UsageMetric,
} from '../../interfaces/user-account.interface';
import {
  digitsValidator,
  focusFirstInvalid,
  stateRegistrationValidator,
} from '../../utils/account-validators';

export interface InvoiceDownload {
  invoice: Invoice;
  document: InvoiceDocument;
}

type TagSeverity = 'success' | 'warn' | 'danger' | 'secondary' | 'info';
type UsageLevel = 'normal' | 'high' | 'critical';

const USAGE_COLORS: Record<UsageLevel, string> = {
  normal: 'var(--p-primary-color)',
  high: 'var(--p-amber-500)',
  critical: 'var(--p-red-500)',
};

const BRAZIL_STATES = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG', 'PA',
  'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO',
];

const INVOICE_SEVERITY: Record<InvoiceStatus, TagSeverity> = {
  paid: 'success',
  pending: 'warn',
  cancelled: 'secondary',
};

const PLAN_SEVERITY: Record<PlanStatus, TagSeverity> = {
  active: 'success',
  trialing: 'info',
  pastDue: 'danger',
  cancelled: 'secondary',
};

/** Tab 3 — plan, usage, billing data and invoice history. */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-account-billing-tab',
  imports: [
    CurrencyPipe,
    DecimalPipe,
    ReactiveFormsModule,
    TranslatePipe,
    ButtonModule,
    FloatLabelModule,
    InputMaskModule,
    ProgressBarModule,
    TableModule,
    TagModule,
    TooltipModule,
    InputFieldComponent,
    SelectFieldComponent,
    FormErrorComponent,
    PlainDatePipe,
  ],
  templateUrl: './account-billing-tab.component.html',
  styleUrl: './account-billing-tab.component.scss',
})
export class AccountBillingTabComponent {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  billing = input.required<AccountBilling>();
  saving = input<boolean>(false);

  saveDetails = output<BillingDetails>();
  upgrade = output<void>();
  cancelPlan = output<void>();
  changeCard = output<void>();
  download = output<InvoiceDownload>();

  protected readonly planSeverity = PLAN_SEVERITY;
  protected readonly stateOptions: Options[] = BRAZIL_STATES.map(uf => ({ code: uf, description: uf }));

  protected readonly form = new FormGroup<BillingDetailsFormControls>({
    legalName: new FormControl(null, [Validators.required, Validators.maxLength(150)]),
    cnpj: new FormControl(null, [Validators.required, digitsValidator([14], 'invalidCnpj')]),
    stateRegistration: new FormControl(null, [Validators.required, stateRegistrationValidator]),
    financeEmail: new FormControl(null, [Validators.required, Validators.email]),
    zipCode: new FormControl(null, [Validators.required, digitsValidator([8], 'invalidZipCode')]),
    street: new FormControl(null, [Validators.required, Validators.maxLength(150)]),
    number: new FormControl(null, [Validators.required, Validators.maxLength(10)]),
    complement: new FormControl(null, [Validators.maxLength(60)]),
    district: new FormControl(null, [Validators.required, Validators.maxLength(80)]),
    city: new FormControl(null, [Validators.required, Validators.maxLength(80)]),
    state: new FormControl(null, [Validators.required]),
  });

  constructor() {
    effect(() => this.populate(this.billing().details));
  }

  protected invoiceSeverityOf(invoice: Invoice): TagSeverity {
    return INVOICE_SEVERITY[invoice.status];
  }

  protected usagePercent(metric: UsageMetric): number {
    return Math.min(100, Math.round((metric.used / metric.limit) * 100));
  }

  /** Amber from 75 %, red from 90 % — the point where an upgrade is worth suggesting. */
  protected usageLevel(metric: UsageMetric): UsageLevel {
    const percent = this.usagePercent(metric);
    if (percent >= 90) return 'critical';
    if (percent >= 75) return 'high';
    return 'normal';
  }

  protected usageColor(metric: UsageMetric): string {
    return USAGE_COLORS[this.usageLevel(metric)];
  }

  protected onReset(): void {
    this.populate(this.billing().details);
  }

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      focusFirstInvalid(this.host.nativeElement);
      return;
    }

    const value = this.form.getRawValue();
    this.saveDetails.emit({
      legalName: value.legalName!.trim(),
      cnpj: value.cnpj!,
      stateRegistration: value.stateRegistration!.trim().toUpperCase(),
      financeEmail: value.financeEmail!.trim(),
      address: {
        zipCode: value.zipCode!,
        street: value.street!.trim(),
        number: value.number!.trim(),
        complement: value.complement?.trim() || null,
        district: value.district!.trim(),
        city: value.city!.trim(),
        state: value.state!,
      },
    });
  }

  private populate(details: BillingDetails): void {
    this.form.reset({
      legalName: details.legalName,
      cnpj: details.cnpj,
      stateRegistration: details.stateRegistration,
      financeEmail: details.financeEmail,
      ...details.address,
    });
  }
}
