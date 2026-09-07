import { Injectable } from '@angular/core';
import { Observable, of, shareReplay } from 'rxjs';
import { StoreBranch } from '../interfaces/checkout.interface';

// Mocked until the branches endpoint exists.
const MOCK_BRANCHES: StoreBranch[] = [
  {
    id: 'b1',
    name: 'Loja Centro — São Paulo',
    address: 'R. XV de Novembro, 101',
    openingHours: 'Seg–Sáb 9h–20h',
  },
  {
    id: 'b2',
    name: 'Loja Pinheiros — São Paulo',
    address: 'R. dos Pinheiros, 847',
    openingHours: 'Seg–Dom 10h–22h',
  },
  {
    id: 'b3',
    name: 'Loja Shopping Morumbi',
    address: 'Av. Roque Petroni Jr., 1089',
    openingHours: 'Seg–Dom 10h–22h',
  },
];

@Injectable({ providedIn: 'root' })
export class StoreBranchService {
  private readonly branches$ = of(MOCK_BRANCHES).pipe(shareReplay(1));

  getBranches(): Observable<StoreBranch[]> {
    return this.branches$;
  }
}
