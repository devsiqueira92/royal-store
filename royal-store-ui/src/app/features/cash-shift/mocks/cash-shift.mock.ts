import { CashMovement } from '../interfaces/cash-movement.interface';

// Mocked until the shift metadata and cash-movement endpoints exist.
export const CASH_SHIFT_META_MOCK = {
  code: 'SHF-2041',
  register: 'Register 1',
};

export const CASH_MOVEMENTS_MOCK: CashMovement[] = [
  {
    id: 'mov-4',
    type: 'sale',
    amount: 92.4,
    description: 'Sale #10419 · cash',
    occurredAt: '2026-09-11T15:58:00',
  },
  {
    id: 'mov-3',
    type: 'withdrawal',
    amount: 300,
    description: 'Bank deposit — Itaú',
    occurredAt: '2026-09-11T14:22:00',
  },
  {
    id: 'mov-2',
    type: 'supplement',
    amount: 100,
    description: 'Change fund top-up',
    occurredAt: '2026-09-11T11:05:00',
  },
  {
    id: 'mov-1',
    type: 'sale',
    amount: 148,
    description: 'Sale #10412 · cash',
    occurredAt: '2026-09-11T09:41:00',
  },
];
