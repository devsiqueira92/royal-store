import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { CostCompositionFacade } from '../../facade/cost-composition.facade';
import { CostCompositionSummary } from '../../interfaces/cost-composition.interface';
import { CostCompositionFormComponent } from '../../components/cost-composition-form/cost-composition-form.component';

@Component({
  imports: [TableModule, TagModule, ButtonModule, TranslatePipe, CurrencyPipe, DatePipe],
  templateUrl: './cost-compositions.component.html',
  styleUrl: './cost-compositions.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CostCompositionsComponent implements OnInit {
  private readonly facade = inject(CostCompositionFacade);
  private readonly sidebar = inject(DetailsSidebarService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translateService = inject(TranslateService);

  protected readonly costCompositions = toSignal(
    this.facade.costCompositions$,
    { initialValue: [] as CostCompositionSummary[] },
  );

  protected readonly isLoading = toSignal(this.facade.isLoading$, {
    initialValue: false,
  });

  ngOnInit(): void {
    this.facade.loadCostCompositions();
  }

  protected create(): void {
    this.facade.clearSelectedCostComposition();
    this.sidebar.openDrawer(CostCompositionFormComponent, {
      title: 'costComposition.form.createTitle',
      subtitle: 'costComposition.form.createSubtitle',
      mode: 'create',
    });
  }

  protected edit(costComposition: CostCompositionSummary): void {
    this.facade.loadCostComposition(costComposition.id);
    this.sidebar.openDrawer(CostCompositionFormComponent, {
      title: 'costComposition.form.editTitle',
      subtitle: 'costComposition.form.editSubtitle',
      mode: 'edit',
    });
  }

  protected onDelete(
    costComposition: CostCompositionSummary,
    event: Event,
  ): void {
    this.confirmationService.confirm({
      target: event.currentTarget as EventTarget,
      message: this.translateService.instant(
        'costComposition.form.deleteConfirm',
      ),
      icon: 'pi pi-info-circle',
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', outlined: true },
      acceptLabel: this.translateService.instant('costComposition.form.delete'),
      rejectLabel: this.translateService.instant('costComposition.form.cancel'),
      accept: () => {
        this.facade.delete(costComposition.id);
        this.facade.clearSelectedCostComposition();
        this.sidebar.closeDrawer();
      },
    });
  }
}
