import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ConfirmationService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { TaxProfileTemplateFacade } from '@app/core/tax-profile-template/facade/tax-profile-template.facade';
import { TaxProfileTemplate } from '@app/core/tax-profile-template/interfaces/tax-profile-template.interface';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { TaxProfileTemplateFormComponent } from '../../components/tax-profile-template-form/tax-profile-template-form.component';

@Component({
  imports: [TableModule, ButtonModule, TagModule, TranslatePipe],
  templateUrl: './tax-profile-templates.component.html',
  styleUrl: './tax-profile-templates.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaxProfileTemplatesComponent implements OnInit {
  private readonly taxProfileTemplateFacade = inject(TaxProfileTemplateFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translateService = inject(TranslateService);

  protected readonly templates = toSignal(
    this.taxProfileTemplateFacade.templates$,
    { initialValue: [] as TaxProfileTemplate[] },
  );

  protected readonly isLoading = toSignal(
    this.taxProfileTemplateFacade.isLoading$,
    { initialValue: false },
  );

  ngOnInit(): void {
    this.taxProfileTemplateFacade.loadTemplates();
  }

  protected targetItemTypeLabel(targetItemType: number): string {
    return targetItemType === 1
      ? 'taxProfileTemplate.targetItemType.service'
      : 'taxProfileTemplate.targetItemType.product';
  }

  protected createTemplate(): void {
    this.taxProfileTemplateFacade.clearSelectedTemplate();
    this.detailsSidebarService.openDrawer(TaxProfileTemplateFormComponent, {
      title: 'taxProfileTemplate.form.create',
      subtitle: 'taxProfileTemplate.form.createSubtitle',
      mode: 'create',
    });
  }

  protected editTemplate(template: TaxProfileTemplate): void {
    this.taxProfileTemplateFacade.selectTemplate(template.id);
    this.detailsSidebarService.openDrawer(TaxProfileTemplateFormComponent, {
      title: 'taxProfileTemplate.form.edit',
      subtitle: 'taxProfileTemplate.form.subtitle',
      mode: 'edit',
    });
  }

  // One-way: the API has no endpoint to reactivate a template once deactivated.
  protected deactivateTemplate(template: TaxProfileTemplate, event: Event): void {
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
      accept: () => this.taxProfileTemplateFacade.deactivateTemplate(template.id),
    });
  }
}
