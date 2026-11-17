import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { PriceListFacade } from '../../facade/price-list.facade';
import { PriceList } from '../../interfaces/price-list.interface';
import { PriceListFormComponent } from '../../components/price-list-form/price-list-form.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { ConfirmationService } from 'primeng/api';
import { RadioButton } from 'primeng/radiobutton';
import { FormsModule } from '@angular/forms';
import { RadioButtonModule } from 'primeng/radiobutton';

@Component({
  imports: [
    TableModule,
    ButtonModule,
    TagModule,
    TranslatePipe,
    SwitchComponent,
    FormsModule,
    RadioButtonModule,
  ],
  templateUrl: './price-lists.component.html',
  styleUrl: './price-lists.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PriceListsComponent implements OnInit {
  private readonly facade = inject(PriceListFacade);
  private readonly sidebar = inject(DetailsSidebarService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translateService = inject(TranslateService);

  protected readonly priceLists = toSignal(this.facade.priceLists$, {
    initialValue: [] as PriceList[],
  });

  protected readonly defaultPriceListId = computed(
    () => this.priceLists().find(p => p.isDefault)?.id ?? null,
  );

  protected readonly isLoading =toSignal(this.facade.isLoading$, {
    initialValue: false,
  });

  ngOnInit(): void {
    this.facade.loadPriceLists();
  }

  protected createPriceList(): void {
    this.facade.clearSelectedPriceList();
    this.sidebar.openDrawer(PriceListFormComponent, {
      title: 'priceList.form.createTitle',
      subtitle: 'priceList.form.createSubtitle',
      mode: 'create',
    });
  }

  protected editPriceList(priceList: PriceList): void {
    this.facade.selectPriceList(priceList.id);
    this.sidebar.openDrawer(PriceListFormComponent, {
      title: 'priceList.form.editTitle',
      subtitle: 'priceList.form.editSubtitle',
      mode: 'edit',
    });
  }

  protected onActiveChange(priceList: PriceList, active: boolean): void {
    if (active && !priceList.isActive) {
      this.facade.activate(priceList.id);
    } else if (!active && priceList.isActive) {
      this.facade.deactivate(priceList.id);
    }
  }

  protected onSetDefault(priceList: PriceList): void {
    if (!priceList.isDefault) {
      this.facade.setDefault(priceList.id);
    }
  }

  protected getRuleLabel(priceList: PriceList): string | null {
    if (
      priceList.discountPercentage == null ||
      priceList.basePriceListName == null
    ) {
      return null;
    }
    const pct = priceList.discountPercentage;
    return `${pct}% · ${priceList.basePriceListName}`;
  }

  protected onDelete(priceList: PriceList, event: Event): void {
    if (priceList.isDefault) {
      return;
    }
    this.confirmationService.confirm({
      target: event.currentTarget as EventTarget,
      message: this.translateService.instant('priceList.form.deleteConfirm', {
        name: priceList.name,
      }),
      icon: 'pi pi-info-circle',
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', outlined: true },
      acceptLabel: this.translateService.instant('priceList.form.delete'),
      rejectLabel: this.translateService.instant('priceList.form.cancel'),
      accept: () => {
        this.facade.delete(priceList.id);
        this.facade.clearSelectedPriceList();
        this.sidebar.closeDrawer();
      },
    });
  }

  setDefaultPriceList(priceList: PriceList): void {
    this.facade.setDefault(priceList.id);
  }
}
