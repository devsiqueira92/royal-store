import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { PriceList } from '../../interfaces/price-list.interface';

/**
 * The catalog's price lists: a strip of chips, and a table to create, rename,
 * activate and pick the default one.
 *
 * Presentational: every change is reported to the parent, which owns the lists.
 * The default list can be neither deactivated nor removed — it is the one the
 * POS falls back to.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-price-list-manager',
  imports: [TranslatePipe, ButtonModule, InputTextModule, SwitchComponent],
  templateUrl: './price-list-manager.component.html',
  styleUrl: './price-list-manager.component.scss',
})
export class PriceListManagerComponent {
  readonly priceLists = input.required<PriceList[]>();

  readonly add = output<string>();
  readonly rename = output<{ id: string; name: string }>();
  readonly defaultChange = output<string>();
  readonly activeChange = output<{ id: string; active: boolean }>();
  readonly remove = output<string>();

  protected readonly managing = signal(false);
  protected readonly newName = signal('');

  protected readonly canAdd = computed(() => {
    const name = this.newName().trim();
    return name.length > 0 && !this.isNameTaken(name, null);
  });

  protected toggleManaging(): void {
    this.managing.update((managing) => !managing);
  }

  protected onNewNameInput(event: Event): void {
    this.newName.set((event.target as HTMLInputElement).value);
  }

  /** Also bound to Enter, which would otherwise submit the product form around it. */
  protected onAdd(event?: Event): void {
    event?.preventDefault();

    if (!this.canAdd()) {
      return;
    }

    this.add.emit(this.newName().trim());
    this.newName.set('');
  }

  protected onRename(list: PriceList, event: Event): void {
    const field = event.target as HTMLInputElement;
    const name = field.value.trim();

    // An empty or duplicated name is not a rename: the field goes back.
    if (!name || this.isNameTaken(name, list.id)) {
      field.value = list.name;
      return;
    }

    if (name !== list.name) {
      this.rename.emit({ id: list.id, name });
    }
  }

  protected onRenameEnter(event: Event): void {
    event.preventDefault();
    (event.target as HTMLInputElement).blur();
  }

  private isNameTaken(name: string, exceptId: string | null): boolean {
    const wanted = name.toLocaleLowerCase();

    return this.priceLists().some(
      (list) => list.id !== exceptId && list.name.trim().toLocaleLowerCase() === wanted,
    );
  }
}
