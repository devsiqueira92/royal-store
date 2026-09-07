import { Injectable } from '@angular/core';
import { Observable, of, shareReplay } from 'rxjs';
import { Customer } from '../interfaces/customer.interface';

// Mocked until the customers endpoint exists — the API only stores customers, it does not list them.
const MOCK_CUSTOMERS: Customer[] = [
  {
    id: 'c1',
    name: 'Ana Carolina Souza',
    phone: '11987650001',
    document: '39053344705',
    tier: 'gold',
    cashbackBalance: 42.5,
  },
  {
    id: 'c2',
    name: 'Bruno Henrique Lima',
    phone: '11987650002',
    document: '12345678909',
    tier: 'vip',
    cashbackBalance: 128,
  },
  {
    id: 'c3',
    name: 'Camila Torres Ferreira',
    phone: '11987650003',
    document: '11144477735',
    tier: 'silver',
    cashbackBalance: 0,
  },
  {
    id: 'c4',
    name: 'Diego Martins Alves',
    phone: '11987650004',
    document: '52998224725',
    tier: 'standard',
    cashbackBalance: 15.9,
  },
];

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly customers$ = of(MOCK_CUSTOMERS).pipe(shareReplay(1));

  getCustomers(): Observable<Customer[]> {
    return this.customers$;
  }
}
