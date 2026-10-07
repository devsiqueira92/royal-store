import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CashShift } from '../interfaces/cash-shift.interface';
import { OpenCashShiftRequest } from '../interfaces/open-cash-shift-request.interface';
import { CloseCashShiftRequest } from '../interfaces/close-cash-shift-request.interface';

@Injectable({
  providedIn: 'root',
})
export class CashShiftService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/cash-shifts`;

  getCurrent(): Observable<CashShift | null> {
    return this.http.get<CashShift | null>(`${this.apiUrl}/current`);
  }

  open(request: OpenCashShiftRequest): Observable<CashShift> {
    debugger;
    return this.http.post<CashShift>(this.apiUrl, request);
  }

  close(request: CloseCashShiftRequest): Observable<CashShift> {
    return this.http.patch<CashShift>(`${this.apiUrl}/close`, request);
  }
}
