import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  untracked,
} from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { map, startWith } from 'rxjs';
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

  protected readonly isCreateMode = computed(
    () => this.sidebar.contentToRender()?.props?.mode === 'create',
  );

  protected readonly selected = toSignal(this.facade.selectedCostComposition$, {
    initialValue: null as CostComposition | null,
  });

  protected readonly nameControl = new FormControl<string | null>(null, [
    Validators.required,
    Validators.maxLength(NAME_MAX_LENGTH),
  ]);

  protected readonly items = new FormArray<FormGroup<CostItemControls>>([]);
  protected readonly formGroup = new FormGroup({
    name: this.nameControl,
    items: this.items,
  });

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

  /** Edit mode waits for GET api/cost-compositions/{id} before it can be saved. */
  protected readonly canSubmit = computed(
    () => this.isCreateMode() || this.selected() !== null,
  );

  /** Id of the composition the form was last filled from; avoids refilling over user edits. */
  private filledId: string | null = null;

  constructor() {
    this.addItem();

    // Edit mode: the full composition (with lines) arrives after the drawer opens.
    effect(() => {
      const selected = this.selected();
      if (this.isCreateMode() || !selected || selected.id === this.filledId) {
        return;
      }
      untracked(() => this.fill(selected));
    });
  }

  private fill(composition: CostComposition): void {
    this.filledId = composition.id;
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
    if (!this.canSubmit()) {
      return;
    }
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

    const selected = this.selected();
    if (this.isCreateMode() || !selected) {
      this.facade.create({ name, items });
    } else {
      this.facade.update(selected.id, { name, items });
    }

    this.onClose();
  }
}
