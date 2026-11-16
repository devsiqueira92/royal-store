import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { PriceListFacade } from '../../facade/price-list.facade';
import { PriceList } from '../../interfaces/price-list.interface';
import { PriceListFormComponent } from '../../components/price-list-form/price-list-form.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';

@Component({
  imports: [
    TableModule,
    ButtonModule,
    TagModule,
    TranslatePipe,
    SwitchComponent,
  ],
  templateUrl: './price-lists.component.html',
  styleUrl: './price-lists.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PriceListsComponent implements OnInit {
  private readonly facade = inject(PriceListFacade);
  private readonly sidebar = inject(DetailsSidebarService);

  protected readonly priceLists = toSignal(this.facade.priceLists$, {
    initialValue: [] as PriceList[],
  });

  protected readonly isLoading = toSignal(this.facade.isLoading$, {
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
}
