import { Routes } from '@angular/router';
import { CashShiftEntryComponent } from './pages/cash-shift-entry/cash-shift-entry.component';
import { CloseCashShiftComponent } from './components/close-cash-shift/close-cash-shift.component';

export const CASH_SHIFT_ROUTES: Routes = [
  { path: '', component: CashShiftEntryComponent },
  { path: 'close', component: CloseCashShiftComponent },
];
