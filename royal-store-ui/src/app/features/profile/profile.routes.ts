import { Routes } from '@angular/router';
import { ProfileComponent } from './pages/profile/profile.component';
import { AccountComponent } from './pages/account/account.component';

export const PROFILE_ROUTES: Routes = [
  { path: '', component: ProfileComponent },
  // Mock-backed account management, for layout validation before the API exists.
  { path: '', component: AccountComponent },
];
