import { inject, Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';

@Injectable({
  providedIn: 'root',
})
export class PopupService {
  private readonly translateService = inject(TranslateService);
  private readonly confirmationService = inject(ConfirmationService);

  openDeleteConfirmation(event: Event, props: any) {
    this.confirmationService.confirm({
      target: event.currentTarget as EventTarget,
      message: this.translateService.instant(props.message),
      icon: 'pi pi-info-circle',
      rejectButtonProps: {
        severity: 'secondary',
        outlined: true,
      },
      acceptButtonProps: {
        severity: 'danger',
      },
      accept: () => {
        return props.accept();
      },

      acceptLabel: props.acceptLabel
        ? props.acceptLabel
        : this.translateService.instant('delete'),

      rejectLabel: this.translateService.instant('cancel'),
    });
  }
}
