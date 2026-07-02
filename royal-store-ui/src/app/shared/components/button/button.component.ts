import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  input,
  Output,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-button',
  imports: [ButtonModule, TooltipModule, TranslatePipe],
  templateUrl: './button.component.html',
  styleUrl: './button.component.scss',
})
export class ButtonComponent {
  icon = input<string>('');
  label = input<string>('');
  variant = input<'outlined' | 'text'>('text');
  rounded = input<boolean>(false);
  disabled = input<boolean>(false);
  text = input<boolean>(false);
  tooltip = input<string>('');
  severity = input<
    'primary' | 'secondary' | 'success' | 'info' | 'warn' | 'help' | 'danger'
  >('primary');

  @Output() onClick = new EventEmitter<void>();
}
