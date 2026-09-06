import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  inject,
  Input,
  Output,
} from '@angular/core';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { TooltipModule } from 'primeng/tooltip';
import { PopupService } from '../../services/popup.service';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-delete-button',
  imports: [CommonModule, TooltipModule, TranslatePipe, ConfirmPopupModule],
  templateUrl: './delete-button.component.html',
  styleUrl: './delete-button.component.scss',
})
export class DeleteButtonComponent {
  @Input() disabled = false;
  @Output() onDelete = new EventEmitter();

  private readonly popupService = inject(PopupService);

  confirmToDelete(event: Event) {
    if (this.disabled) return;

    this.popupService.openDeleteConfirmation(event, {
      message: 'popup.deleteConfirmation',
      accept: () => {
        this.onDelete.emit();
      },
    });
  }
}
