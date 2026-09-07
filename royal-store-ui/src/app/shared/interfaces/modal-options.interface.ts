import { FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { ActionCreator } from '@ngrx/store';
import { DataMode } from '../enum/data-mode.enum';

export interface ModalOptions {
  header?: string;
  width?: string;
  height?: string;
  data?: {
    form?: FormGroup;
    header?: string;
    mode?: DataMode;
    body?: string;
    title?: string;
    loading?: Observable<boolean>;
    onConfirm?: any;
  };
  successAction?: ActionCreator<string, any>;
  okButton?: {
    click: () => void;
    label: string;
  };

  backButton?: {
    label: string;
  };
}
