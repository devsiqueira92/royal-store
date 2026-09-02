import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { Panel } from 'primeng/panel';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-form-layout',
  imports: [TranslatePipe, Panel],
  templateUrl: './form-layout.component.html',
  styleUrl: './form-layout.component.scss',
})
export class FormLayoutComponent {
  index = input<number>(1);
  icon = input<string>();
  title = input.required<string>();
  headerDetails = input<string>();
  collapsedPanel = input<boolean>(true);
  toggleable = input<boolean>(true);
}
