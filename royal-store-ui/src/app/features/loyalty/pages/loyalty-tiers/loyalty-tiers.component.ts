import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmationService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { LoyaltyTierFacade } from '@app/core/loyalty-tier/facade/loyalty-tier.facade';
import { LoyaltyTier } from '@app/core/loyalty-tier/interfaces/loyalty-tier.interface';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { LoyaltyTierFormComponent } from '../../components/loyalty-tier-form/loyalty-tier-form.component';

@Component({
  imports: [TableModule, ButtonModule, TranslatePipe, SwitchComponent],
  templateUrl: './loyalty-tiers.component.html',
  styleUrl: './loyalty-tiers.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoyaltyTiersComponent implements OnInit {
  private readonly loyaltyTierFacade = inject(LoyaltyTierFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translateService = inject(TranslateService);

  protected readonly loyaltyTiers = toSignal(
    this.loyaltyTierFacade.loyaltyTiers$,
    { initialValue: [] as LoyaltyTier[] },
  );

  protected readonly isLoading = toSignal(this.loyaltyTierFacade.isLoading$, {
    initialValue: false,
  });

  ngOnInit(): void {
    this.loyaltyTierFacade.loadLoyaltyTiers();
  }

  protected createTier(): void {
    this.loyaltyTierFacade.clearSelectedLoyaltyTier();
    this.detailsSidebarService.openDrawer(LoyaltyTierFormComponent, {
      title: 'loyalty.form.create',
      subtitle: 'loyalty.form.createSubtitle',
      mode: 'create',
    });
  }

  protected editTier(tier: LoyaltyTier): void {
    this.loyaltyTierFacade.loadLoyaltyTier(tier.id);
    this.detailsSidebarService.openDrawer(LoyaltyTierFormComponent, {
      title: 'loyalty.form.edit',
      subtitle: 'loyalty.form.subtitle',
      mode: 'edit',
    });
  }

  protected onActiveChange(tier: LoyaltyTier, active: boolean): void {
    if (active === tier.isActive) {
      return;
    }

    if (active) {
      this.loyaltyTierFacade.activate(tier.id);
    } else {
      this.loyaltyTierFacade.deactivate(tier.id);
    }
  }

  protected deleteTier(tier: LoyaltyTier, event: Event): void {
    this.confirmationService.confirm({
      target: event.currentTarget as EventTarget,
      message: this.translateService.instant('loyalty.deleteConfirm', {
        name: tier.name,
      }),
      icon: 'pi pi-info-circle',
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', outlined: true },
      acceptLabel: this.translateService.instant('loyalty.form.delete'),
      rejectLabel: this.translateService.instant('loyalty.form.cancel'),
      accept: () => this.loyaltyTierFacade.deleteLoyaltyTier(tier.id),
    });
  }
}
