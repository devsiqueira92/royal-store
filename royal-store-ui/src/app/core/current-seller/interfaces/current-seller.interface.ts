import { FormControl } from '@angular/forms';

/** The signed-in user's seller profile, as returned by `GET/PUT /api/sellers/me`. */
export interface CurrentSeller {
  fullName: string;
  phone: string | null;
  email: string | null;
  isActive: boolean;
  // ISO 8601 date-time (DateTimeOffset on the API).
  createdAt: string;
  isProfileComplete: boolean;
}

/** Body for `PUT /api/sellers/me` (`UpdateMyProfileCommand`). */
export interface UpdateMyProfileRequest {
  fullName: string;
  phone: string | null;
  email: string | null;
}

export interface ProfileFormControls {
  fullName: FormControl<string | null>;
  email: FormControl<string | null>;
  phone: FormControl<string | null>;
}
