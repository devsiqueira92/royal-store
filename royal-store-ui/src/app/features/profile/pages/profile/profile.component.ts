import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';
import { AvatarModule } from 'primeng/avatar';
import { CurrentSellerFacade } from '@app/core/current-seller/facade/current-seller.facade';
import { CurrentSeller } from '@app/core/current-seller/interfaces/current-seller.interface';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-profile',
  imports: [DatePipe, TranslatePipe, AvatarModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  private readonly currentSellerFacade = inject(CurrentSellerFacade);

  protected readonly currentSeller = toSignal(
    this.currentSellerFacade.currentSeller$,
    { initialValue: null as CurrentSeller | null },
  );
  protected readonly isLoading = toSignal(this.currentSellerFacade.isLoading$, {
    initialValue: false,
  });

  ngOnInit(): void {
    this.currentSellerFacade.loadCurrentSeller();
  }

  protected initials(fullName: string): string {
    return fullName
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part[0])
      .join('')
      .toUpperCase();
  }
}
