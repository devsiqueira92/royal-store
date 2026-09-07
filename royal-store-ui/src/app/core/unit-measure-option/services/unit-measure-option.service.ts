import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { UnitMeasureOption } from '../interfaces/unit-measure-option.interface';

@Injectable({
  providedIn: 'root',
})
export class UnitMeasureOptionService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/unit-measure-options`;

  getAll(): Observable<UnitMeasureOption[]> {
    return this.http.get<UnitMeasureOption[]>(this.apiUrl);
  }
}
