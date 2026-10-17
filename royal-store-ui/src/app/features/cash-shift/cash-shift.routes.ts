import { Routes } from '@angular/router';
import { CashShiftEntryComponent } from './pages/cash-shift-entry/cash-shift-entry.component';
import { CashShiftDetailsComponent } from './components/cash-shift-details/cash-shift-details.component';
import { CashShiftListComponent } from './components/cash-shift-list/cash-shift-list.component';

// Static paths must stay above ':id'.
export const CASH_SHIFT_ROUTES: Routes = [
  { path: '', component: CashShiftListComponent },
  { path: 'current', component: CashShiftEntryComponent },
  { path: 'management', component: CashShiftDetailsComponent },
  { path: ':id', component: CashShiftDetailsComponent },
];
