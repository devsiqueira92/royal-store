import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';
import { BYPASS_GLOBAL_ERROR } from '@app/core/auth/interceptor/bypass-error.context';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const messageService = inject(NotificationService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (req.context.get(BYPASS_GLOBAL_ERROR)) {
        return throwError(() => error);
      }

      switch (error.status) {
        case 401:
          router.navigate(['/login']);
          break;
        case 403:
          messageService.error({
            summary: 'Forbidden',
            detail: 'You do not have permission to perform this action.',
          });
          break;
        case 0:
          messageService.error({
            summary: 'Network Error',
            detail: 'Unable to reach the server. Check your connection.',
          });
          break;
        default:
          messageService.error({
            summary: 'Error',
            detail: error.error?.message ?? 'An unexpected error occurred.',
          });
      }

      return throwError(() => error);
    }),
  );
};
