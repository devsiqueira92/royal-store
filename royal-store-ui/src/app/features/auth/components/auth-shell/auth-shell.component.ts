import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

/**
 * Frame shared by every authentication screen.
 *
 * Before this, each page (login, register, verify, forgot, reset) centred its
 * own card on an empty white page: no brand, no context, and five copies of the
 * same layout rules. The shell gives all of them one brand panel, one card
 * treatment and one way back to the public site, so the screens stay consistent
 * as new ones are added.
 *
 * It is presentational: pages project their form into `<ng-content>`.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-auth-shell',
  imports: [RouterLink, TranslatePipe],
  templateUrl: './auth-shell.component.html',
  styleUrl: './auth-shell.component.scss',
})
export class AuthShellComponent {
  protected readonly bulletKeys = [
    'auth.shell.bullet1',
    'auth.shell.bullet2',
    'auth.shell.bullet3',
  ];
}
