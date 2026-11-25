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
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { map, startWith } from 'rxjs';
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
} from '../../interfaces/label-format.interface';
import {
  computeLabelSheetLayout,
  toLabelSheetSpec,
} from '../../utils/label-sheet-layout.util';
import { DetailLayoutComponent } from '@app/shared/components/layouts/detail-layout/detail-layout.component';

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
    ConfirmDialog,
    DetailLayoutComponent,
  ],
  templateUrl: './label-designer.component.html',
  styleUrl: './label-designer.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelDesignerComponent {
  private readonly productFacade = inject(ProductFacade);
  private readonly priceListFacade = inject(PriceListFacade);
  private readonly labelFacade = inject(LabelFacade);
  private readonly router = inject(Router);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translateService = inject(TranslateService);

  /** Key of this page's confirm dialog, so the app-wide confirm popup ignores it. */
  protected readonly labelTypeConfirmKey = 'labelDesignerLabelType';

  /** Template id from `labels/designer/:id`; `null` → creating a new label. */
  protected readonly labelId =
    inject(ActivatedRoute).snapshot.paramMap.get('id');

  protected readonly isSaving = toSignal(this.labelFacade.isLoading$, {
    initialValue: false,
  });

  private readonly selectedLabel = toSignal(this.labelFacade.selectedLabel$, {
    initialValue: null,
  });

  /** The stored label being edited, once `GET api/labels/{id}` answered. */
  protected readonly editedLabel = computed<Label | null>(() => {
    const label = this.selectedLabel();
    return this.labelId && label?.template.id === this.labelId ? label : null;
  });

  /** "Start from" options (`GET api/labels/configurations`), loaded by the format panel. */
  private readonly configurations = this.labelFacade.configurations;

  private readonly loadedConfiguration = this.labelFacade.selectedConfiguration;

  protected readonly selectionForm =
    new FormGroup<LabelDesignerSelectionControls>({
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
    columns: new FormControl<number | null>(1, [
      Validators.required,
      Validators.min(1),
    ]),
    horizontalSpacing: mm(),
    verticalSpacing: mm(),
    marginTop: mm(),
    marginRight: mm(),
    marginBottom: mm(),
    marginLeft: mm(),
  });

  /**
   * Configuration in effect: the stored label's source while editing, then the last pick
   * applied (confirmed, in edit mode). "Voltar" on the confirm dialog reverts the dropdown to it.
   */
  private readonly appliedLabelTypeId = signal<string | null>(null);

  /** Full label of the applied configuration, once `GET api/labels/{id}` answered. */
  protected readonly selectedConfiguration = computed<Label | null>(() => {
    const configuration = this.loadedConfiguration();
    const id = this.appliedLabelTypeId();
    return id && configuration?.template.id === id ? configuration : null;
  });

  private readonly sheetSpec = toSignal(
    this.sheetForm.valueChanges.pipe(
      startWith(null),
      map(() => toLabelSheetSpec(this.sheetForm.getRawValue())),
    ),
    { initialValue: toLabelSheetSpec(this.sheetForm.getRawValue()) },
  );

  protected readonly layout = computed(() =>
    computeLabelSheetLayout(this.sheetSpec()),
  );

  private readonly priceLists = toSignal(this.priceListFacade.priceLists$, {
    initialValue: [] as PriceList[],
  });

  private readonly selectedProducts = toSignal(
    this.productFacade.selectedLabelProducts$,
    {
      initialValue: [] as Product[],
    },
  );

  protected readonly fieldOptions = computed(() =>
    buildLabelFieldOptions(this.priceLists()),
  );

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
      this.selectedConfiguration()?.format.paperType ??
      this.editedLabel()?.format.paperType ??
      null,
  );

  protected readonly canSave = computed(
    () =>
      !this.isSaving() &&
      (this.editedLabel() !== null || this.selectedConfiguration() !== null),
  );

  constructor() {
    this.priceListFacade.loadPriceLists();
    // Drop a configuration left from a previous visit.
    this.labelFacade.clearSelectedConfiguration();

    if (this.labelId) {
      // Drop any previous selection so only this fetch fills the screen.
      this.labelFacade.clearSelectedLabel();
      this.labelFacade.loadLabel(this.labelId);
    }

    // Fill the screen once with the stored label's name, configuration, measurements and elements.
    let applied = false;
    effect(() => {
      const label = this.editedLabel();
      if (!label || applied) {
        return;
      }
      applied = true;
      untracked(() => {
        this.selectionForm.controls.name.setValue(label.template.name);
        // Shows the stored source without loading it over the label's own layout.
        this.appliedLabelTypeId.set(label.template.sourceTemplateId);
        this.selectionForm.controls.labelTypeId.setValue(
          label.template.sourceTemplateId,
          { emitEvent: false },
        );
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
      .subscribe(
        () => void this.router.navigateByUrl('/products/labels/templates'),
      );

    // Picking a configuration fetches its full label; while editing, only once the user
    // confirms that the current measurements and elements are discarded.
    this.selectionForm.controls.labelTypeId.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(id => {
        if (this.editedLabel()) {
          this.confirmLabelTypeChange(id);
        } else {
          this.applyLabelType(id);
        }
      });

    // Once it arrives it fills the measurements and the editor's elements. The name is
    // left to the user.
    effect(() => {
      const configuration = this.selectedConfiguration();
      if (!configuration) {
        return;
      }
      untracked(() => {
        this.applyFormat(configuration.format);
        this.elements.set(
          normalizeLabelElements(
            parseLabelConfiguration(configuration.template.configuration),
            this.priceLists(),
          ),
        );
      });
    });

    // Price lists may arrive after the template was loaded: map the legacy
    // `product.price` field to the default list then, so its toggle shows as active.
    effect(() => {
      const priceLists = this.priceLists();
      untracked(() =>
        this.elements.update(elements =>
          normalizeLabelElements(elements, priceLists),
        ),
      );
    });

    // New label: preselect the first configuration (system ones come first) once the list arrives.
    effect(() => {
      const [initial] = this.configurations();
      const control = this.selectionForm.controls.labelTypeId;
      if (this.labelId || !initial || control.value) {
        return;
      }
      untracked(() => control.setValue(initial.code));
    });
  }

  private applyLabelType(id: string | null): void {
    this.appliedLabelTypeId.set(id);
    if (id) {
      this.labelFacade.loadConfiguration(id);
    } else {
      this.labelFacade.clearSelectedConfiguration();
    }
  }

  /** Edit mode: "Confirmar" applies the pick; "Voltar" (or closing) restores the previous one. */
  private confirmLabelTypeChange(id: string | null): void {
    const t = (key: string): string =>
      this.translateService.instant(
        `product.labelDesigner.changePreset.${key}`,
      );
    this.confirmationService.confirm({
      key: this.labelTypeConfirmKey,
      header: t('header'),
      message: t('message'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: t('confirm'),
      rejectLabel: t('back'),
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', outlined: true },
      accept: () => this.applyLabelType(id),
      reject: () =>
        this.selectionForm.controls.labelTypeId.setValue(
          this.appliedLabelTypeId(),
          { emitEvent: false },
        ),
    });
  }

  /** Adds the field to the label, or removes every element bound to it. */
  protected onFieldToggled(option: LabelFieldOption): void {
    const current = this.elements();
    if (current.some(e => e.field === option.field)) {
      this.elements.set(current.filter(e => e.field !== option.field));
      return;
    }
    const { labelWidth, labelHeight } = this.sheetSpec();
    this.elements.set([
      ...current,
      createLabelElement(option, current, labelWidth, labelHeight),
    ]);
  }

  /** Position, width, font size or alignment of one element changed in the editor. */
  protected onElementChanged({ index, changes }: LabelElementChange): void {
    this.elements.update(list =>
      list.map((element, i) =>
        i === index ? { ...element, ...changes } : element,
      ),
    );
  }

  /**
   * Stored label → `PUT api/labels/{id}` (a pre-configuration loaded on top only changes
   * its paper and source). New label → `POST api/labels` from the chosen pre-configuration.
   */
  protected onSave(): void {
    const edited = this.editedLabel();
    const preset = this.selectedConfiguration();
    const context = edited
      ? {
          ...saveContextFromLabel(edited),
          ...(preset && {
            paperType: preset.format.paperType,
            sourceTemplateId: preset.template.id,
          }),
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
    const request = buildSaveLabelTemplateRequest(
      context,
      name,
      this.sheetSpec(),
      this.elements(),
    );

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
