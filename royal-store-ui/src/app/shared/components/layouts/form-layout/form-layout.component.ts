import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-form-layout',
  imports: [TranslatePipe],
  templateUrl: './form-layout.component.html',
  styleUrl: './form-layout.component.scss',
})
export class FormLayoutComponent {
  index = input<number>(1);
  header = input.required<string>();
  headerDetails = input<string | number>();
}
