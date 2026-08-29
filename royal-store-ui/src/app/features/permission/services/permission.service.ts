import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PermissionCatalogResponse } from '../interfaces/permission-catalog.interface';

@Injectable({
  providedIn: 'root',
})
export class PermissionService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/permissions`;

  getCatalog(): Observable<PermissionCatalogResponse> {
    return this.http.get<PermissionCatalogResponse>(this.apiUrl);
  }
}
