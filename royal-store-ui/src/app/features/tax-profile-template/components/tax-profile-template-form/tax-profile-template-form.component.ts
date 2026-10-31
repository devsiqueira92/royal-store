import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { TaxProfileTemplateFacade } from '@app/core/tax-profile-template/facade/tax-profile-template.facade';
import {
  TaxProfileTemplate,
  TaxProfileTemplateFormControls,
  TaxProfileTemplateTargetItemType,
} from '@app/core/tax-profile-template/interfaces/tax-profile-template.interface';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { TextareaFieldComponent } from '@app/shared/components/form-fields/textarea-field/textarea-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { ToggleFieldComponent } from '@app/shared/components/form-fields/toggle-field/toggle-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { Options } from '@app/shared/interceptors/options.interface';

@Component({
  selector: 'app-tax-profile-template-form',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    TagModule,
    ButtonModule,
    InputFieldComponent,
    TextareaFieldComponent,
    SelectFieldComponent,
    ToggleFieldComponent,
    FormLayoutComponent,
    ButtonComponent,
  ],
  templateUrl: './tax-profile-template-form.component.html',
  styleUrl: './tax-profile-template-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaxProfileTemplateFormComponent {
  private readonly taxProfileTemplateFacade = inject(TaxProfileTemplateFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translateService = inject(TranslateService);

  protected readonly isCreateMode = computed(
    () => this.detailsSidebarService.contentToRender()?.props?.mode === 'create',
  );

  protected readonly selectedTemplate = toSignal(
    this.taxProfileTemplateFacade.selectedTemplate$,
    { initialValue: null as TaxProfileTemplate | null },
  );

  // Deactivation is one-way — the API has no endpoint to reactivate a template.
  protected readonly isActive = computed(() => this.selectedTemplate()?.isActive ?? true);

  protected readonly targetItemTypeOptions: Options[] = [
    { code: '0', description: this.translateService.instant('taxProfileTemplate.targetItemType.product') },
    { code: '1', description: this.translateService.instant('taxProfileTemplate.targetItemType.service') },
  ];

  protected readonly formGroup = this.buildFormGroup();

  constructor() {
    effect(() => {
      if (this.isCreateMode()) {
        this.enterCreateMode();
        return;
      }

      this.populateForm(this.selectedTemplate());
    });
  }

  protected onClose(): void {
    this.taxProfileTemplateFacade.clearSelectedTemplate();
    this.detailsSidebarService.closeDrawer();
  }

  protected onDeactivate(event: Event): void {
    const template = this.selectedTemplate();
    if (!template || !template.isActive) {
      return;
    }

    this.confirmationService.confirm({
      target: event.currentTarget as EventTarget,
      message: this.translateService.instant('taxProfileTemplate.deactivateConfirm', {
        name: template.name,
      }),
      icon: 'pi pi-info-circle',
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', outlined: true },
      acceptLabel: this.translateService.instant('taxProfileTemplate.form.deactivate'),
      rejectLabel: this.translateService.instant('taxProfileTemplate.form.cancel'),
      accept: () => {
        this.taxProfileTemplateFacade.deactivateTemplate(template.id);
        this.onClose();
      },
    });
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const {
      name,
      description,
      targetItemType,
      defaultCfop,
      icmsCst,
      pisCofinsCst,
      ivaCst,
      isSelectiveTax,
      isDefault,
    } = this.formGroup.getRawValue();

    if (this.isCreateMode()) {
      this.taxProfileTemplateFacade.createTemplate({
        name: (name ?? '').trim(),
        description: this.normalize(description),
        targetItemType: Number(targetItemType ?? 0) as TaxProfileTemplateTargetItemType,
        defaultCfop: this.normalize(defaultCfop),
        icmsCst: this.normalize(icmsCst),
        pisCofinsCst: this.normalize(pisCofinsCst),
        ivaCst: this.normalize(ivaCst),
        isSelectiveTax,
        isDefault,
      });
    } else {
      const template = this.selectedTemplate();
      if (!template) {
        return;
      }

      this.taxProfileTemplateFacade.updateTemplate(template.id, {
        name: (name ?? '').trim(),
        description: this.normalize(description),
        defaultCfop: this.normalize(defaultCfop),
        icmsCst: this.normalize(icmsCst),
        pisCofinsCst: this.normalize(pisCofinsCst),
        ivaCst: this.normalize(ivaCst),
        isSelectiveTax,
        isDefault,
      });
    }

    this.onClose();
  }

  private normalize(value: string | null | undefined): string | undefined {
    const trimmed = value?.trim();
    return trimmed ? trimmed : undefined;
  }

  private enterCreateMode(): void {
    this.formGroup.controls.targetItemType.enable();
    this.formGroup.reset({
      name: null,
      description: null,
      targetItemType: '0',
      defaultCfop: null,
      icmsCst: null,
      pisCofinsCst: null,
      ivaCst: null,
      isSelectiveTax: false,
      isDefault: false,
    });
  }

  private populateForm(template: TaxProfileTemplate | null): void {
    // targetItemType is immutable once set.
    this.formGroup.controls.targetItemType.disable();
    this.formGroup.reset({
      name: template?.name ?? null,
      description: template?.description ?? null,
      targetItemType: String(template?.targetItemType ?? 0),
      defaultCfop: template?.defaultCfop ?? null,
      icmsCst: template?.icmsCst ?? null,
      pisCofinsCst: template?.pisCofinsCst ?? null,
      ivaCst: template?.ivaCst ?? null,
      isSelectiveTax: template?.isSelectiveTax ?? false,
      isDefault: template?.isDefault ?? false,
    });
  }

  private buildFormGroup(): FormGroup<TaxProfileTemplateFormControls> {
    return new FormGroup<TaxProfileTemplateFormControls>({
      name: new FormControl(null, [Validators.required, Validators.maxLength(100)]),
      description: new FormControl(null, [Validators.maxLength(500)]),
      targetItemType: new FormControl('0', [Validators.required]),
      defaultCfop: new FormControl(null, [Validators.maxLength(4)]),
      icmsCst: new FormControl(null, [Validators.maxLength(4)]),
      pisCofinsCst: new FormControl(null, [Validators.maxLength(4)]),
      ivaCst: new FormControl(null, [Validators.maxLength(4)]),
      isSelectiveTax: new FormControl(false, { nonNullable: true }),
      isDefault: new FormControl(false, { nonNullable: true }),
    });
  }
}
