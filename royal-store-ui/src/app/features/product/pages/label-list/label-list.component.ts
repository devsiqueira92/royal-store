import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { LabelFacade } from '../../facade/label.facade';
import { LabelPaperType, LabelSummary } from '../../interfaces/label-format.interface';

/** Saved labels (`GET api/labels`), with activate/deactivate and delete. */
@Component({
  imports: [
    TableModule,
    TagModule,
    ButtonModule,
    TooltipModule,
    RouterLink,
    TranslatePipe,
    DatePipe,
    DecimalPipe,
  ],
  templateUrl: './label-list.component.html',
  styleUrl: './label-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelListComponent implements OnInit {
  private readonly facade = inject(LabelFacade);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translateService = inject(TranslateService);

  protected readonly labels = toSignal(this.facade.labels$, {
    initialValue: [] as LabelSummary[],
  });
  protected readonly isLoading = toSignal(this.facade.isLoading$, {
    initialValue: false,
  });

  protected readonly PaperType = LabelPaperType;

  ngOnInit(): void {
    this.facade.loadLabels();
  }

  protected toggleActive(label: LabelSummary): void {
    const { id, isActive } = label;
    if (isActive) {
      this.facade.deactivate(id);
    } else {
      this.facade.activate(id);
    }
  }

  protected onDelete(label: LabelSummary, event: Event): void {
    this.confirmationService.confirm({
      target: event.currentTarget as EventTarget,
      message: this.translateService.instant('product.labelList.deleteConfirm'),
      icon: 'pi pi-info-circle',
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', outlined: true },
      acceptLabel: this.translateService.instant('product.labelList.delete'),
      rejectLabel: this.translateService.instant('product.labelList.cancel'),
      accept: () => this.facade.delete(label.id),
    });
  }
}
