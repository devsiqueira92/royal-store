import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  AutoCompleteModule,
  AutoCompleteCompleteEvent,
  AutoCompleteSelectEvent,
} from 'primeng/autocomplete';
import { FloatLabelModule } from 'primeng/floatlabel';
import { IbptTaxFacade } from '../../../../core/ibpt-tax/facade/ibpt-tax.facade';
import { IbptTax } from '../../../../core/ibpt-tax/interfaces/ibpt-tax.interface';

const MIN_QUERY_LENGTH = 3;

@Component({
  selector: 'app-ibpt-tax-autocomplete',
  imports: [FormsModule, TranslatePipe, AutoCompleteModule, FloatLabelModule],
  templateUrl: './ibpt-tax-autocomplete.component.html',
  styleUrl: './ibpt-tax-autocomplete.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IbptTaxAutocompleteComponent {
  private readonly facade = inject(IbptTaxFacade);

  /** Current code held by the parent form. */
  code = input<string | null>(null);
  label = input<string>('product.form.ncm');
  /** 0 = NCM (products) → &type=0 ; 1 = NBS (services) → &type=1 */
  type = input<number>(0);

  /** Emits the selected IBPT tax `code`. */
  codeSelected = output<string>();

  protected readonly minLength = MIN_QUERY_LENGTH;

  protected readonly suggestions = toSignal(this.facade.results$, {
    initialValue: [] as IbptTax[],
  });

  protected readonly model = signal<IbptTax | string | null>(null);

  constructor() {
    // Keep the visible value in sync with the parent's code.
    effect(() => {
      const code = this.code();
      const current = this.model();
      const currentCode =
        typeof current === 'string' ? current : (current?.code ?? null);

      if (code !== currentCode) {
        this.model.set(code ?? null);
      }
    });
  }

  protected onComplete(event: AutoCompleteCompleteEvent): void {
    const query = (event.query ?? '').trim();

    if (query.length < MIN_QUERY_LENGTH) {
      this.facade.clear();
      return;
    }

    // Each call replaces the previous search; the effect uses switchMap to
    // cancel the in-flight request.
    this.facade.search(query, this.type());
  }

  protected onSelect(event: AutoCompleteSelectEvent): void {
    const tax = event.value as IbptTax;
    this.model.set(tax);
    this.codeSelected.emit(tax.code);
  }
}
