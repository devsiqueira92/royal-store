import {
  afterNextRender,
  DestroyRef,
  Directive,
  effect,
  ElementRef,
  inject,
  input,
  output,
  untracked,
} from '@angular/core';

/**
 * Put on a sentinel element after a list: emits `reachedEnd` when it scrolls into view (067 FR-013).
 * While `infiniteScrollDisabled` is true nothing is emitted. When it turns false, the sentinel's
 * visibility is measured again on the next frame — after the page just loaded is laid out — so it
 * emits only if the list still does not fill the screen, never from a visibility seen before the
 * new rows pushed the sentinel down.
 */
@Directive({
  selector: '[appInfiniteScroll]',
})
export class InfiniteScrollDirective {
  readonly infiniteScrollDisabled = input(false);
  /** Starts loading this far before the sentinel is actually visible. */
  readonly infiniteScrollMargin = input('200px');
  readonly reachedEnd = output<void>();

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
  private observer: IntersectionObserver | null = null;

  constructor() {
    const destroyRef = inject(DestroyRef);

    afterNextRender(() => {
      this.observer = new IntersectionObserver(
        ([entry]) => {
          if (entry.isIntersecting && !this.infiniteScrollDisabled()) {
            this.reachedEnd.emit();
          }
        },
        { rootMargin: this.infiniteScrollMargin() },
      );
      this.observer.observe(this.host);
      destroyRef.onDestroy(() => this.observer?.disconnect());
    });

    effect(() => {
      if (!this.infiniteScrollDisabled()) {
        untracked(() => this.remeasure());
      }
    });
  }

  /** Re-observing makes the observer report the sentinel's current visibility on the next frame. */
  private remeasure(): void {
    if (!this.observer) {
      return;
    }
    this.observer.unobserve(this.host);
    this.observer.observe(this.host);
  }
}
