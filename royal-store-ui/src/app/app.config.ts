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
import { sellerFeature } from './features/seller/store/seller.reducer';
import { sellerStore } from './features/seller/store/seller.store';
import { productFeature } from './features/product/store/product.reducer';
import { productStore } from './features/product/store/product.store';
import { categoryFeature } from './features/category/store/category.reducer';
import { categoryStore } from './features/category/store/category.store';
import { permissionFeature } from './features/permission/store/permission.reducer';
import { permissionStore } from './features/permission/store/permission.store';
import { userFeature } from './features/user/store/user.reducer';
import { userStore } from './features/user/store/user.store';
import { saleFeature } from './features/sale/store/sale.reducer';
import { saleStore } from './features/sale/store/sale.store';
import { ibptTaxFeature } from './core/ibpt-tax/store/ibpt-tax.reducer';
import { ibptTaxStore } from './core/ibpt-tax/store/ibpt-tax.store';
import { taxProfileTemplateFeature } from './core/tax-profile-template/store/tax-profile-template.reducer';
import { taxProfileTemplateStore } from './core/tax-profile-template/store/tax-profile-template.store';
import { unitMeasureOptionFeature } from './core/unit-measure-option/store/unit-measure-option.reducer';
import { unitMeasureOptionStore } from './core/unit-measure-option/store/unit-measure-option.store';
import { customerFeature } from './features/customer/store/customer.reducer';
import { customerStore } from './features/customer/store/customer.store';
import { loyaltyTierFeature } from './core/loyalty-tier/store/loyalty-tier.reducer';
import { loyaltyTierStore } from './core/loyalty-tier/store/loyalty-tier.store';
import { originOptionFeature } from './core/origin-option/store/origin-option.reducer';
import { originOptionStore } from './core/origin-option/store/origin-option.store';
import { paymentMethodFeature } from './features/payment-method/store/payment-method.reducer';
import { paymentMethodStore } from './features/payment-method/store/payment-method.store';

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
    provideStore({
      [authFeature.name]: authFeature.reducer,
      [sellerFeature.name]: sellerFeature.reducer,
      [productFeature.name]: productFeature.reducer,
      [categoryFeature.name]: categoryFeature.reducer,
      [permissionFeature.name]: permissionFeature.reducer,
      [userFeature.name]: userFeature.reducer,
      [ibptTaxFeature.name]: ibptTaxFeature.reducer,
      [saleFeature.name]: saleFeature.reducer,
      [taxProfileTemplateFeature.name]: taxProfileTemplateFeature.reducer,
      [unitMeasureOptionFeature.name]: unitMeasureOptionFeature.reducer,
      [customerFeature.name]: customerFeature.reducer,
      [loyaltyTierFeature.name]: loyaltyTierFeature.reducer,
      [originOptionFeature.name]: originOptionFeature.reducer,
      [paymentMethodFeature.name]: paymentMethodFeature.reducer,
    }),
    provideEffects(
      authStore,
      sellerStore,
      productStore,
      categoryStore,
      permissionStore,
      userStore,
      ibptTaxStore,
      saleStore,
      taxProfileTemplateStore,
      unitMeasureOptionStore,
      customerStore,
      loyaltyTierStore,
      originOptionStore,
      paymentMethodStore,
    ),
    { provide: LOCALE_ID, useValue: 'en-US' },
    DialogService,
    DynamicDialogRef,
    MessageService,
    ConfirmationService,
  ],
};
