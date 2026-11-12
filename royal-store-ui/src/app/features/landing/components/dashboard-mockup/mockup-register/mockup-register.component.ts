import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

interface CartLine {
  readonly id: string;
  readonly nameKey: string;
  readonly qty: number;
  readonly price: string;
}

/**
 * Simulated register screen, a child of `DashboardMockupComponent`.
 *
 * It lives in its own component because the register is the densest of the four
 * mockups: separating it keeps each screen's markup and stylesheet small enough
 * to read (and inside the project's per-component style budget).
 */
@Component({
  selector: 'app-mockup-register',
  imports: [TranslatePipe],
  templateUrl: './mockup-register.component.html',
  styleUrl: './mockup-register.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MockupRegisterComponent {
  protected readonly cartLines: readonly CartLine[] = [
    { id: 'c1', nameKey: 'landing.mockup.products.p1', qty: 2, price: '58,00' },
    { id: 'c2', nameKey: 'landing.mockup.products.p5', qty: 1, price: '24,90' },
    { id: 'c3', nameKey: 'landing.mockup.products.p2', qty: 3, price: '91,50' },
    { id: 'c4', nameKey: 'landing.mockup.products.p6', qty: 1, price: '12,40' },
  ];
}
