import { Component, inject, Injectable, Type } from '@angular/core';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { take } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { Actions } from '@ngrx/effects';

@Injectable({ providedIn: 'root' })
export class DialogManagerService {
  private readonly dialogService = inject(DialogService);
  private readonly translateService = inject(TranslateService);
  private readonly actions$ = inject(Actions);
  private openDialogs: DynamicDialogRef[] = [];

  private registerDialog(ref: DynamicDialogRef) {
    this.openDialogs.push(ref);

    ref.onClose.pipe(take(1)).subscribe(() => {
      this.openDialogs = this.openDialogs.filter(r => r !== ref);
    });
  }

  closeAll() {
    this.openDialogs.forEach(ref => ref.close());
    this.openDialogs = [];
  }

  open(component: Type<unknown>, props?: {}) {
    const ref = this.dialogService.open(component, {
      modal: true,
      closeOnEscape: true,
      closable: false,
      width: '600px',
      height: '775px',
      baseZIndex: 100,
      header: this.translateService.instant(`modal.header.warning`),
      ...props,
    })!;

    this.registerDialog(ref);
    return ref;
  }
}
