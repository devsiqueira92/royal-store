import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '@root/src/environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const isApiUrl = req.url.startsWith(environment.apiUrl);
  if (isApiUrl) {
    const authReq = req.clone({
      withCredentials: true,
    });

    return next(authReq);
  }
  return next(req);
};
