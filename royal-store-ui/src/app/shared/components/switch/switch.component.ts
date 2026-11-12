import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToggleSwitchModule, ToggleSwitchChangeEvent } from 'primeng/toggleswitch';

@Component({
  selector: 'app-switch',
  imports: [FormsModule, ToggleSwitchModule],
  template: `
    <p-toggleswitch
      [ngModel]="checked()"
      [disabled]="disabled()"
      [ariaLabel]="ariaLabel()"
      (onChange)="onToggle($event)"
    />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SwitchComponent {
  checked = input<boolean>(false);
  disabled = input<boolean>(false);
  ariaLabel = input<string>('');

  changed = output<boolean>();

  protected onToggle(event: ToggleSwitchChangeEvent): void {
    this.changed.emit(!!event.checked);
  }
}
