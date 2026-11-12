import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  input,
  output,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { RippleModule } from 'primeng/ripple';
import { LandingNavLink } from '../../models/landing.model';

/**
 * Fixed glass navbar. Presentational: the parent page owns the smooth-scroll
 * behaviour, this component only reports which section was requested.
 *
 * The mobile panel keeps its open/closed flag locally because that is pure UI
 * state with no meaning outside this component.
 */
@Component({
  selector: 'app-landing-navbar',
  imports: [RouterLink, TranslatePipe, RippleModule],
  templateUrl: './landing-navbar.component.html',
  styleUrl: './landing-navbar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingNavbarComponent {
  readonly links = input.required<readonly LandingNavLink[]>();
  /** Section currently in view, highlighted in the nav. */
  readonly activeSection = input<string | null>(null);

  readonly navigateToSection = output<string>();

  protected readonly scrolled = signal(false);
  protected readonly menuOpen = signal(false);

  /** Guards against doing work on every scroll event (one read per frame). */
  private scrollQueued = false;

  @HostListener('window:scroll')
  protected onWindowScroll(): void {
    if (this.scrollQueued) {
      return;
    }

    this.scrollQueued = true;
    requestAnimationFrame(() => {
      this.scrollQueued = false;
      this.scrolled.set(window.scrollY > 12);
    });
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    this.menuOpen.set(false);
  }

  protected toggleMenu(): void {
    this.menuOpen.update(open => !open);
  }

  protected onLinkClick(event: Event, sectionId: string): void {
    // Keep the href for middle-click / "open in new tab", but handle the
    // in-page jump ourselves so the router is never involved.
    event.preventDefault();
    this.menuOpen.set(false);
    this.navigateToSection.emit(sectionId);
  }
}
