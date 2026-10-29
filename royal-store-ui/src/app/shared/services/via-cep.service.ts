import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  filter,
  map,
  Observable,
  of,
  switchMap,
} from 'rxjs';
import { BYPASS_GLOBAL_ERROR } from '@app/core/auth/interceptor/bypass-error.context';
import { ViaCepAddress, ViaCepResponse } from '../interfaces/via-cep.interface';

@Injectable({
  providedIn: 'root',
})
export class ViaCepService {
  private http = inject(HttpClient);
  private readonly apiUrl = 'https://viacep.com.br/ws';

  /**
   * @param zipCode the 8 digits, no separator
   * @returns null when ViaCEP doesn't know the zip code or can't be reached
   */
  lookup(zipCode: string): Observable<ViaCepAddress | null> {
    return this.http
      .get<ViaCepResponse>(`${this.apiUrl}/${zipCode}/json/`, {
        // The caller reports the miss itself; the generic error toast would be noise.
        context: new HttpContext().set(BYPASS_GLOBAL_ERROR, true),
      })
      .pipe(
        map(response => ('erro' in response ? null : response)),
        catchError(() => of(null)),
      );
  }

  /**
   * Looks up each complete, well-formed zip code the user settles on in
   * `control`. Disabled controls are skipped, so values set by code while the
   * field is read-only never trigger a request.
   */
  lookupChanges(
    control: AbstractControl<string | null>,
  ): Observable<ViaCepAddress | null> {
    return control.valueChanges.pipe(
      debounceTime(300),
      map(value => value?.replace(/\D/g, '') ?? ''),
      distinctUntilChanged(),
      filter(digits => digits.length === 8 && control.enabled && control.valid),
      switchMap(digits => this.lookup(digits)),
    );
  }
}
