import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { TranslateService } from '@ngx-translate/core';
@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private translate = inject(TranslateService);
  protected readonly title = signal('royal-store-ui');

  constructor() {
    this.translate.addLangs(['en']);
  }
}
