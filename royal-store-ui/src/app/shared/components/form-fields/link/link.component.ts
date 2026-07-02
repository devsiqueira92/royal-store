import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  selector: 'app-link',
  templateUrl: './link.component.html',
  styleUrl: './link.component.scss',
})
export class LinkComponent {
  readonly routerLink = input<string | any[] | null>(null);
  readonly queryParams = input<any>(null);
  readonly href = input<string | null>(null);
  readonly target = input<'_blank' | '_self' | '_parent' | '_top'>('_self');

  readonly variant = input<'inline' | 'btn-primary' | 'btn-secondary'>(
    'inline',
  );
  readonly customClass = input<string>('');

  readonly classesCss = computed(() => {
    return `link-base variant-${this.variant()} ${this.customClass()}`;
  });
}
