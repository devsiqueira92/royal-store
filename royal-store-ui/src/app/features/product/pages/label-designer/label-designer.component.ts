import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  untracked,
} from '@angular/core';
import { PriceListFacade } from '@app/features/price-list/facade/price-list.facade';
import { PriceList } from '@app/features/price-list/interfaces/price-list.interface';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { LabelEditorComponent } from '../../components/label-editor/label-editor.component';
import {
  buildSaveLabelTemplateRequest,
  parseLabelConfiguration,
  saveContextFromLabel,
  saveContextFromPreset,
} from '../../utils/label-template-payload.util';
import { ProductFacade } from '../../facade/product.facade';
import { LabelFacade } from '../../facade/label.facade';
import { Product } from '../../interfaces/product.interface';
import {
  buildLabelFieldOptions,
  buildMockLabelSampleProduct,
  createLabelElement,
  LabelElementChange,
  LabelFieldOption,
  LabelSampleProduct,
  normalizeLabelElements,
  toLabelSampleProduct,
} from '../../utils/label-field.util';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { map, startWith } from 'rxjs';
import { Options } from '@app/shared/interceptors/options.interface';
import { LabelFormatPanelComponent } from '../../components/label-format-panel/label-format-panel.component';
import { LabelSheetPreviewComponent } from '../../components/label-sheet-preview/label-sheet-preview.component';
import {
  LABEL_NAME_MAX_LENGTH,
  Label,
  LabelDesignerSelectionControls,
  LabelElement,
  LabelFormat,
  LabelSheetFormControls,
  LabelSheetSpec,
  LabelType,
} from '../../interfaces/label-format.interface';
import { LabelFormatService } from '../../services/label-format.service';
import { computeLabelSheetLayout, toLabelSheetSpec } from '../../utils/label-sheet-layout.util';

const mm = (): FormControl<number | null> =>
  new FormControl<number | null>(0, [Validators.required, Validators.min(0)]);

/**
 * New label printing screen (replaces `LabelPrintComponent` once complete).
 * Left: label name, pre-configuration loader and sheet/label measurements.
 * Center: label editor. Right: live outline of the sheet.
 */
@Component({
  imports: [
    TranslatePipe,
    RouterLink,
    ButtonComponent,
    LabelFormatPanelComponent,
    LabelSheetPreviewComponent,
    LabelEditorComponent,
  ],
  templateUrl: './label-designer.component.html',
  styleUrl: './label-designer.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelDesignerComponent {
  private readonly labelFormatService = inject(LabelFormatService);
  private readonly productFacade = inject(ProductFacade);
  private readonly priceListFacade = inject(PriceListFacade);
  private readonly labelFacade = inject(LabelFacade);
  private readonly router = inject(Router);

  /** Template id from `labels/designer/:id`; `null` → creating a new label. */
  protected readonly labelId = inject(ActivatedRoute).snapshot.paramMap.get('id');

  protected readonly isSaving = toSignal(this.labelFacade.isLoading$, { initialValue: false });

  private readonly selectedLabel = toSignal(this.labelFacade.selectedLabel$, {
    initialValue: null,
  });

  /** The stored label being edited, once `GET api/labels/{id}` answered. */
  protected readonly editedLabel = computed<Label | null>(() => {
    const label = this.selectedLabel();
    return this.labelId && label?.template.id === this.labelId ? label : null;
  });

  private readonly labelTypes$ = this.labelFormatService.getLabelTypes();

  protected readonly labelTypes = toSignal<LabelType[] | null>(this.labelTypes$, {
    initialValue: null,
  });

  protected readonly isLoading = computed(() => this.labelTypes() === null);

  protected readonly labelTypeOptions = computed<Options[]>(() =>
    (this.labelTypes() ?? []).map((t) => ({ code: t.id, description: t.name })),
  );

  protected readonly selectionForm = new FormGroup<LabelDesignerSelectionControls>({
    name: new FormControl<string | null>(null, [
      Validators.required,
      Validators.maxLength(LABEL_NAME_MAX_LENGTH),
    ]),
    labelTypeId: new FormControl<string | null>(null),
  });

  protected readonly sheetForm = new FormGroup<LabelSheetFormControls>({
    paperWidth: mm(),
    paperHeight: mm(),
    labelWidth: mm(),
    labelHeight: mm(),
    columns: new FormControl<number | null>(1, [Validators.required, Validators.min(1)]),
    horizontalSpacing: mm(),
    verticalSpacing: mm(),
    marginTop: mm(),
    marginRight: mm(),
    marginBottom: mm(),
    marginLeft: mm(),
  });

  private readonly labelTypeId = toSignal(
    this.selectionForm.controls.labelTypeId.valueChanges.pipe(
      startWith(this.selectionForm.controls.labelTypeId.value),
    ),
    { initialValue: null as string | null },
  );

  protected readonly selectedLabelType = computed(
    () => this.labelTypes()?.find((t) => t.id === this.labelTypeId()) ?? null,
  );

  private readonly sheetSpec = toSignal(
    this.sheetForm.valueChanges.pipe(
      startWith(null),
      map(() => toLabelSheetSpec(this.sheetForm.getRawValue())),
    ),
    { initialValue: toLabelSheetSpec(this.sheetForm.getRawValue()) },
  );

  protected readonly layout = computed(() => computeLabelSheetLayout(this.sheetSpec()));

  private readonly priceLists = toSignal(this.priceListFacade.priceLists$, {
    initialValue: [] as PriceList[],
  });

  private readonly selectedProducts = toSignal(this.productFacade.selectedLabelProducts$, {
    initialValue: [] as Product[],
  });

  protected readonly fieldOptions = computed(() => buildLabelFieldOptions(this.priceLists()));

  protected readonly isMockProduct = computed(() => this.selectedProducts().length === 0);

  /** First product picked on the products page; a mock while none was picked. */
  protected readonly sampleProduct = computed<LabelSampleProduct>(() => {
    const [product] = this.selectedProducts();
    const priceLists = this.priceLists();
    return product
      ? toLabelSampleProduct(product, priceLists)
      : buildMockLabelSampleProduct(priceLists);
  });

  /** Elements of the label being edited; seeded from the chosen template's configuration. */
  protected readonly elements = signal<LabelElement[]>([]);

  /** A loaded pre-configuration decides the paper; otherwise the stored label's. */
  protected readonly paperType = computed(
    () =>
      this.selectedLabelType()?.format.paperType ?? this.editedLabel()?.format.paperType ?? null,
  );

  protected readonly canSave = computed(
    () => !this.isSaving() && (this.editedLabel() !== null || this.selectedLabelType() !== null),
  );

  constructor() {
    this.priceListFacade.loadPriceLists();

    if (this.labelId) {
      // Drop any previous selection so only this fetch fills the screen.
      this.labelFacade.clearSelectedLabel();
      this.labelFacade.loadLabel(this.labelId);
    }

    // Fill the screen once with the stored label's name, measurements and elements.
    let applied = false;
    effect(() => {
      const label = this.editedLabel();
      if (!label || applied) {
        return;
      }
      applied = true;
      untracked(() => {
        this.selectionForm.controls.name.setValue(label.template.name);
        this.applyFormat(label.format);
        this.elements.set(
          normalizeLabelElements(
            parseLabelConfiguration(label.template.configuration),
            this.priceLists(),
          ),
        );
      });
    });

    // Back to the saved labels once the API accepts the new one.
    this.labelFacade.saved$
      .pipe(takeUntilDestroyed())
      .subscribe(() => void this.router.navigateByUrl('/products/labels/templates'));

    // Loading a pre-configuration fills the name (still editable), the measurements
    // and the editor's elements.
    this.selectionForm.controls.labelTypeId.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((id) => {
        const labelType = this.labelTypes()?.find((t) => t.id === id);
        if (labelType) {
          this.selectionForm.controls.name.setValue(labelType.name);
          this.applyFormat(labelType.format);
          this.elements.set(
            normalizeLabelElements([...labelType.configuration.elements], this.priceLists()),
          );
        }
      });

    // Price lists may arrive after the template was loaded: map the legacy
    // `product.price` field to the default list then, so its toggle shows as active.
    effect(() => {
      const priceLists = this.priceLists();
      untracked(() =>
        this.elements.update((elements) => normalizeLabelElements(elements, priceLists)),
      );
    });

    // New label: preselect the default (or first) type once the list arrives.
    effect(() => {
      const types = this.labelTypes();
      const control = this.selectionForm.controls.labelTypeId;
      if (this.labelId || !types?.length || control.value) {
        return;
      }
      const initial = types.find((t) => t.isDefault) ?? types[0];
      control.setValue(initial.id);
    });
  }

  /** Adds the field to the label, or removes every element bound to it. */
  protected onFieldToggled(option: LabelFieldOption): void {
    const current = this.elements();
    if (current.some((e) => e.field === option.field)) {
      this.elements.set(current.filter((e) => e.field !== option.field));
      return;
    }
    const { labelWidth, labelHeight } = this.sheetSpec();
    this.elements.set([...current, createLabelElement(option, current, labelWidth, labelHeight)]);
  }

  /** Position, width, font size or alignment of one element changed in the editor. */
  protected onElementChanged({ index, changes }: LabelElementChange): void {
    this.elements.update((list) =>
      list.map((element, i) => (i === index ? { ...element, ...changes } : element)),
    );
  }

  /**
   * Stored label → `PUT api/labels/{id}` (a pre-configuration loaded on top only changes
   * its paper and source). New label → `POST api/labels` from the chosen pre-configuration.
   */
  protected onSave(): void {
    const edited = this.editedLabel();
    const preset = this.selectedLabelType();
    const context = edited
      ? {
          ...saveContextFromLabel(edited),
          ...(preset && { paperType: preset.format.paperType, sourceTemplateId: preset.id }),
        }
      : preset && saveContextFromPreset(preset);
    if (!context) {
      return;
    }
    if (this.selectionForm.invalid || this.sheetForm.invalid) {
      this.selectionForm.markAllAsTouched();
      this.sheetForm.markAllAsTouched();
      return;
    }

    const name = (this.selectionForm.controls.name.value ?? '').trim();
    const request = buildSaveLabelTemplateRequest(context, name, this.sheetSpec(), this.elements());

    if (edited) {
      this.labelFacade.update(edited.template.id, request);
    } else {
      this.labelFacade.create(request);
    }
  }

  private applyFormat(format: LabelFormat): void {
    const spec: LabelSheetSpec = {
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
    };
    this.sheetForm.setValue(spec);
  }
}
