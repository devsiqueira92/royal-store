import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { AsyncPipe, CurrencyPipe } from '@angular/common';
import { Observable } from 'rxjs';
import { TableModule } from 'primeng/table';
import { TranslatePipe } from '@ngx-translate/core';
import { ProductFacade } from '../../facade/product.facade';
import { Product } from '../../interfaces/product.interface';

@Component({
  imports: [TableModule, TranslatePipe, CurrencyPipe, AsyncPipe],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductsComponent implements OnInit {
  private productFacade = inject(ProductFacade);

  protected readonly products$: Observable<Product[]> =
    this.productFacade.products$;
  protected readonly isLoading$ = this.productFacade.isLoading$;

  ngOnInit(): void {
    this.productFacade.loadProducts();
  }
}
