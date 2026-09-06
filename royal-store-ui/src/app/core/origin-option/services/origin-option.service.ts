import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { OriginOption } from '../interfaces/origin-option.interface';

@Injectable({
  providedIn: 'root',
})
export class OriginOptionService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/origin-options`;

  getAll(): Observable<OriginOption[]> {
    return this.http.get<OriginOption[]>(this.apiUrl);
  }
}
