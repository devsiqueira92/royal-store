import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  untracked,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { startWith } from 'rxjs';
import { PriceListFacade } from '@app/features/price-list/facade/price-list.facade';
import { PriceList } from '@app/features/price-list/interfaces/price-list.interface';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { LabelSheetPrintComponent } from '../../components/label-sheet-print/label-sheet-print.component';
import { LabelFacade } from '../../facade/label.facade';
import { ProductFacade } from '../../facade/product.facade';
import { Label, LabelElement, LabelSheetSpec } from '../../interfaces/label-format.interface';
import { Product } from '../../interfaces/product.interface';
import { normalizeLabelElements } from '../../utils/label-field.util';
import { buildLabelPrintItems } from '../../utils/label-print.util';
import { parseLabelConfiguration } from '../../utils/label-template-payload.util';

interface LabelPrintSelectionControls {
  labelId: FormControl<string | null>;
}

const toSheetSpec = ({ format }: Label): LabelSheetSpec => ({
  paperWidth: format.paperWidth,
  paperHeight: format.paperHeight,
  labelWidth: format.labelWidth,
  labelHeight: format.labelHeight,
  columns: format.columns,
  horizontalSpacing: format.horizontalSpacing,
  verticalSpacing: format.verticalSpacing,
  marginTop: format.marginTop,
  marginRight: format.marginRight,
  marginBottom: format.marginBottom,
  marginLeft: format.marginLeft,
});

/**
 * Prints the products picked on the products page with a saved label:
 * pick the label, its sheet/label format and template are applied to every product.
 */
@Component({
  imports: [TranslatePipe, RouterLink, SelectFieldComponent, LabelSheetPrintComponent],
  templateUrl: './label-print.component.html',
  styleUrl: './label-print.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelPrintComponent {
  private readonly productFacade = inject(ProductFacade);
  private readonly labelFacade = inject(LabelFacade);
  private readonly priceListFacade = inject(PriceListFacade);

  protected readonly selectionForm = new FormGroup<LabelPrintSelectionControls>({
    labelId: new FormControl<string | null>(null),
  });

  /** Store's active labels (default first) + system configurations. */
  protected readonly labelOptions = this.labelFacade.printOptions;

  protected readonly isLoadingLabels = this.labelFacade.isLoadingPrintOptions;

  private readonly labelId = toSignal(
    this.selectionForm.controls.labelId.valueChanges.pipe(
      startWith(this.selectionForm.controls.labelId.value),
    ),
    { initialValue: null as string | null },
  );

  private readonly loadedLabel = toSignal(this.labelFacade.selectedLabel$, {
    initialValue: null as Label | null,
  });

  /** Full label (format + layout) of the picked option, once `GET api/labels/{id}` returns it. */
  protected readonly selectedLabel = computed(() => {
    const label = this.loadedLabel();
    return label && label.template.id === this.labelId() ? label : null;
  });

  private readonly priceLists = toSignal(this.priceListFacade.priceLists$, {
    initialValue: [] as PriceList[],
  });

  private readonly selectedProducts = toSignal(this.productFacade.selectedLabelProducts$, {
    initialValue: [] as Product[],
  });

  protected readonly spec = computed<LabelSheetSpec | null>(() => {
    const label = this.selectedLabel();
    return label ? toSheetSpec(label) : null;
  });

  /** The template's elements; the legacy `product.price` field mapped to the default list. */
  protected readonly elements = computed<LabelElement[]>(() => {
    const label = this.selectedLabel();
    return label
      ? normalizeLabelElements(
          parseLabelConfiguration(label.template.configuration),
          this.priceLists(),
        )
      : [];
  });

  protected readonly items = computed(() =>
    buildLabelPrintItems(this.selectedProducts(), this.priceLists()),
  );

  constructor() {
    this.labelFacade.loadPrintOptions();
    this.priceListFacade.loadPriceLists();

    // Preselect the first option (API lists the store's default first) once the list arrives.
    effect(() => {
      const options = this.labelOptions();
      const control = this.selectionForm.controls.labelId;
      if (!options.length || options.some((o) => o.code === control.value)) {
        return;
      }
      untracked(() => control.setValue(options[0].code));
    });

    // The listing has no layout: fetch the full label for the picked option.
    effect(() => {
      const id = this.labelId();
      if (id && untracked(this.loadedLabel)?.template.id !== id) {
        this.labelFacade.loadLabel(id);
      }
    });
  }
}
