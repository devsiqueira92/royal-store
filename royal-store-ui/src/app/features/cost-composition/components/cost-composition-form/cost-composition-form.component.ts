import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AsyncPipe, CurrencyPipe } from '@angular/common';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { filter, map, startWith } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { CostCompositionFacade } from '../../facade/cost-composition.facade';
import {
  CostComposition,
  CostCompositionItemInput,
} from '../../interfaces/cost-composition.interface';

interface CostItemControls {
  id: FormControl<string | null>;
  description: FormControl<string | null>;
  value: FormControl<number | null>;
}

const DESCRIPTION_MAX_LENGTH = 100;
const NAME_MAX_LENGTH = 100;

@Component({
  selector: 'app-cost-composition-form',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    AsyncPipe,
    CurrencyPipe,
    ButtonModule,
    FormLayoutComponent,
    ButtonComponent,
    InputFieldComponent,
    InputNumberFieldComponent,
  ],
  templateUrl: './cost-composition-form.component.html',
  styleUrl: './cost-composition-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CostCompositionFormComponent {
  private readonly facade = inject(CostCompositionFacade);
  private readonly sidebar = inject(DetailsSidebarService);

  /** Null while creating; the edited composition otherwise. */
  private readonly idControl = new FormControl<string | null>(null);

  protected readonly nameControl = new FormControl<string | null>(null, [
    Validators.required,
    Validators.maxLength(NAME_MAX_LENGTH),
  ]);

  protected readonly items = new FormArray<FormGroup<CostItemControls>>([]);
  protected readonly formGroup = new FormGroup({
    id: this.idControl,
    name: this.nameControl,
    items: this.items,
  });

  /**
   * The composition being edited; null while creating (the list clears it before opening the
   * drawer). Its lines arrive with GET api/cost-compositions/{id}, after the drawer opens; the form
   * is filled once per composition, so a later store update never wipes what was typed.
   */
  private readonly selectedCostComposition$ =
    this.facade.selectedCostComposition$.pipe(
      filter(
        (composition): composition is CostComposition =>
          !!composition && composition.id !== this.idControl.value,
      ),
    );

  /** While an edited composition loads, its form would be sent as a new one — so it waits. */
  protected readonly isLoadingSelected$ = this.facade.isLoadingSelected$;

  protected readonly total = toSignal(
    this.items.valueChanges.pipe(
      startWith(null),
      map(() =>
        this.items.controls.reduce(
          (sum, group) => sum + (group.controls.value.value ?? 0),
          0,
        ),
      ),
    ),
    { initialValue: 0 },
  );

  constructor() {
    this.addItem();

    this.selectedCostComposition$
      .pipe(takeUntilDestroyed())
      .subscribe(composition => this.fill(composition));

    // Closed only once the API accepted the change; on failure the drawer stays open with the input.
    this.facade.saved$.pipe(takeUntilDestroyed()).subscribe(() => this.onClose());
  }

  private fill(composition: CostComposition): void {
    this.idControl.setValue(composition.id);
    this.nameControl.setValue(composition.name);
    this.items.clear();
    (composition.items ?? []).forEach(i => this.addItem(i));
    if (this.items.length === 0) {
      this.addItem();
    }
  }

  protected addItem(item?: CostCompositionItemInput | null): void {
    this.items.push(
      new FormGroup<CostItemControls>({
        id: new FormControl(item?.id ?? null),
        description: new FormControl(item?.description ?? null, [
          Validators.required,
          Validators.maxLength(DESCRIPTION_MAX_LENGTH),
        ]),
        value: new FormControl(item?.value ?? null, [
          Validators.required,
          Validators.min(0),
        ]),
      }),
    );
  }

  protected removeItem(index: number): void {
    this.items.removeAt(index);
  }

  protected onClose(): void {
    this.facade.clearSelectedCostComposition();
    this.sidebar.closeDrawer();
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const name = (this.nameControl.value ?? '').trim();
    const items: CostCompositionItemInput[] = this.items
      .getRawValue()
      .map(i => ({
        id: i.id,
        description: (i.description ?? '').trim(),
        value: i.value ?? 0,
      }));

    const id = this.idControl.value;
    if (id) {
      this.facade.update(id, { name, items });
    } else {
      this.facade.create({ name, items });
    }
  }
}
