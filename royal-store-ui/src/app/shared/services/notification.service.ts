import { inject, Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';
import { Notification } from '../interfaces/notification.interface';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  messageService = inject(MessageService);

  error(details: Notification) {
    this.messageService.add({
      severity: 'error',
      ...details,
    });
  }

  success(details: Notification) {
    this.messageService.add({
      severity: 'success',
      ...details,
    });
  }

  warn(details: Notification) {
    this.messageService.add({
      severity: 'warn',
      ...details,
    });
  }
}
