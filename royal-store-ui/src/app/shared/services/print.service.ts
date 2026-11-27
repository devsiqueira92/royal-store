import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';

export interface PrintElementOptions {
  /** CSS `@page` size, e.g. `80mm auto` for a thermal roll. */
  pageSize?: string;
}

/**
 * Prints a single element in isolation: it is copied, with the app's stylesheets, into a hidden
 * iframe and printed from there. Whatever surrounds it on screen (dialogs, masks, the page behind)
 * never reaches the paper, and no global `@media print` rules are needed.
 */
@Injectable({ providedIn: 'root' })
export class PrintService {
  private readonly document = inject(DOCUMENT);

  printElement(element: HTMLElement, options: PrintElementOptions = {}): void {
    const frame = this.document.createElement('iframe');
    frame.setAttribute('aria-hidden', 'true');
    frame.tabIndex = -1;
    Object.assign(frame.style, {
      position: 'fixed',
      width: '0',
      height: '0',
      border: '0',
      right: '0',
      bottom: '0',
    });

    // Component styles are scoped by attributes copied along with the element's markup, so
    // carrying every stylesheet over is enough for it to render exactly as on screen.
    const styles = Array.from(
      this.document.head.querySelectorAll('style, link[rel="stylesheet"]'),
    )
      .map(node => node.outerHTML)
      .join('');
    const page = options.pageSize
      ? `<style>@page { size: ${options.pageSize}; margin: 0; }</style>`
      : '';

    frame.srcdoc =
      `<!doctype html><html><head><meta charset="utf-8">` +
      `<base href="${this.document.baseURI}">${styles}${page}` +
      // Other features' global print rules (e.g. label sheets hide `body *`) must not
      // reach this document, which holds nothing but the element being printed.
      `<style>html, body { margin: 0; background: #fff; }` +
      `@media print { body * { visibility: visible !important; } }</style>` +
      `</head><body>${element.outerHTML}</body></html>`;

    // `load` fires once the copied stylesheets are in, so the print never catches it unstyled.
    frame.addEventListener('load', () => {
      const view = frame.contentWindow;
      if (!view) {
        frame.remove();
        return;
      }

      view.addEventListener('afterprint', () => frame.remove(), { once: true });
      view.focus();
      view.print();
    });

    this.document.body.appendChild(frame);
  }
}
