import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
  LOCALE_ID,
} from '@angular/core';
import { registerLocaleData } from '@angular/common';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import localeEn from '@angular/common/locales/en';
import { errorInterceptor } from './shared/interceptors/error.interceptor';
import { ConfirmationService, MessageService } from 'primeng/api';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { authFeature } from './core/auth/store/auth.reducer';
import { authStore } from './core/auth/store/auth.store';
import { authInterceptor } from './core/auth/interceptor/auth.interceptor';

registerLocaleData(localeEn);

export const appConfig: ApplicationConfig = {
  providers: [
    providePrimeNG({
      theme: {
        preset: Aura,
      },
    }),
    provideTranslateService({
      loader: provideTranslateHttpLoader({
        prefix: './assets/i18n/',
        suffix: '.json',
      }),
      fallbackLang: 'en',
      lang: 'en',
    }),
    provideHttpClient(withInterceptors([errorInterceptor, authInterceptor])),
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideStore({ [authFeature.name]: authFeature.reducer }),
    provideEffects(authStore),
    { provide: LOCALE_ID, useValue: 'en-US' },
    DialogService,
    DynamicDialogRef,
    MessageService,
    ConfirmationService,
  ],
};
