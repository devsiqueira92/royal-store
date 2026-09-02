import { AbstractControl } from '@angular/forms';

export function toggleControl(
  control: AbstractControl,
  enabled: boolean,
): void {
  if (enabled) {
    control.enable({ emitEvent: false });
  } else {
    control.disable({ emitEvent: false });
  }
}
