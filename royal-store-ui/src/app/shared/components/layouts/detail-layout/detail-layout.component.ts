import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { RouterLink } from '@angular/router';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-detail-layout',
  imports: [TranslatePipe, RouterLink],
  templateUrl: './detail-layout.component.html',
  styleUrl: './detail-layout.component.scss',
})
export class DetailLayoutComponent {
  router = input.required<string[]>();
  title = input.required<string>();
  backText = input('back');
  subtitle = input<string>();
}
