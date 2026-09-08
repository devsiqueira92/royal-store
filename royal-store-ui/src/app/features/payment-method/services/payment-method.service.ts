import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaymentMethod } from '../interfaces/payment-method.interface';
import { UpdatePaymentDiscountRequest } from '../interfaces/update-payment-discount-request.interface';

@Injectable({
  providedIn: 'root',
})
export class PaymentMethodService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/payments`;

  getAll(): Observable<PaymentMethod[]> {
    return this.http.get<PaymentMethod[]>(this.apiUrl);
  }

  updateDiscount(
    id: string,
    request: UpdatePaymentDiscountRequest,
  ): Observable<PaymentMethod> {
    return this.http.patch<PaymentMethod>(
      `${this.apiUrl}/${id}/discount`,
      request,
    );
  }

  activate(id: string): Observable<PaymentMethod> {
    return this.http.patch<PaymentMethod>(`${this.apiUrl}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<PaymentMethod> {
    return this.http.patch<PaymentMethod>(
      `${this.apiUrl}/${id}/deactivate`,
      {},
    );
  }
}
