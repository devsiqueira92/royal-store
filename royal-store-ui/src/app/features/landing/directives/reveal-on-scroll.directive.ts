import {
  DestroyRef,
  Directive,
  ElementRef,
  afterNextRender,
  inject,
  input,
  signal,
} from '@angular/core';

/**
 * Reveals the host element once it scrolls into view (fade + 16px rise).
 *
 * Why a directive instead of a scroll listener: `IntersectionObserver` runs off
 * the main-thread scroll path, so no layout read happens per scroll frame. The
 * animated properties are `opacity`/`transform` only (no reflow, no CLS), and
 * `prefers-reduced-motion` short-circuits straight to the revealed state so the
 * content is never hidden from users who asked for less motion.
 *
 * The transition itself lives in the landing page stylesheet (`.reveal`), so the
 * directive only owns the "is it visible yet" state and the stagger delay.
 */
@Directive({
  selector: '[appRevealOnScroll]',
  host: {
    class: 'reveal',
    '[class.is-revealed]': 'revealed()',
    '[style.--reveal-delay]': 'delayMs() + "ms"',
  },
})
export class RevealOnScrollDirective {
  /** Stagger delay in milliseconds, for lists and grids revealed together. */
  readonly delayMs = input<number>(0, { alias: 'revealDelay' });

  protected readonly revealed = signal(false);

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  constructor() {
    const destroyRef = inject(DestroyRef);

    afterNextRender(() => {
      const prefersReducedMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches;

      if (prefersReducedMotion || typeof IntersectionObserver === 'undefined') {
        this.revealed.set(true);
        return;
      }

      const observer = new IntersectionObserver(
        entries => {
          if (entries.some(entry => entry.isIntersecting)) {
            this.revealed.set(true);
            observer.disconnect();
          }
        },
        // Fire slightly before the element is fully on screen so the motion
        // finishes as the user arrives at it, instead of starting late.
        { rootMargin: '0px 0px -12% 0px', threshold: 0.08 },
      );

      observer.observe(this.host.nativeElement);
      destroyRef.onDestroy(() => observer.disconnect());
    });
  }
}
